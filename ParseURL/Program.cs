using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ParseURL
{
    static class Program
    {
        static HtmlWeb _web = new HtmlWeb();
        static Dictionary<string, Order> Orders = new Dictionary<string, Order>();
        static Dictionary<string, Family> Families = new Dictionary<string, Family>();
        const string _topUrl = "https://www.mbr-pwrc.usgs.gov/id/framlst/infocenter.html";
        const string _framesUrl = "https://www.mbr-pwrc.usgs.gov/id/framlst";
        const string _domain = "https://www.mbr-pwrc.usgs.gov";
        
        static void Run(string url)
        {
            List<string[]> topLevelData = GetTopLevelData(url);
            List<string[]> frameUrls = GetContentFrameURLs(topLevelData);
            CreateBird(frameUrls);
        }

        //Create Bird Classes
        static void CreateBird(List<string[]> urls)
        {
            using (var context = new Context())
            {
                //Create Bird and HtmlDocument instances
                //Remember the first and second elements in the array are the order and family respectively
                foreach (string[] dataBundle in urls)
                {
                    Bird bird = new Bird();
                    if (Orders.ContainsKey(dataBundle[0]))
                    {
                        bird.Order = Orders[dataBundle[0]];
                    }
                    else
                    {
                        Order order = new Order(dataBundle[0]);
                        bird.Order = order;
                        order.Birds.Add(bird);
                        Orders.Add(dataBundle[0], order);
                        context.Orders.Add(order);
                    }

                    if (Families.ContainsKey(dataBundle[1]))
                    {
                        bird.Family = Families[dataBundle[1]];
                    }
                    else
                    {
                        Family family = new Family(dataBundle[1]);
                        Families.Add(dataBundle[1], family);
                        family.Birds.Add(bird);

                        context.Families.Add(family);
                    }

                    HtmlWeb web = _web;
                    try
                    {
                        HtmlDocument document = web.Load(dataBundle[2]);
                        AssignName(bird, document);//
                        AssignScientificName(bird, document);
                        List<string> descripText = AssignLengths(bird, document);
                        AssignIdText(bird, document, descripText);
                        AssignColors(bird, document);
                    }
                    catch (WebException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine($"Failed to load content frame: {dataBundle[2]}");
                        throw e;
                    }

                    try
                    {
                        HtmlDocument document = web.Load(dataBundle[3]);
                        AssignImages(bird, document);
                        context.Birds.Add(bird);
                    }
                    catch (WebException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine($"Failed to load images frame: {dataBundle[3]}");
                        throw e;
                    }
                    context.SaveChanges();
                    break;
                }
                //***REMOVE IN PRODUCTION***
                
            }       
        }

        //***BIRD METHODS***

        static void AssignName(Bird bird, HtmlDocument document)
        {
            string innerTitle = document.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
            string name = innerTitle.Replace("Identification tips", string.Empty);
            bird.Name = name.TrimEnd(' ');
        }

        static void AssignScientificName(Bird bird, HtmlDocument document)
        {
            string h1Inner = document.DocumentNode.SelectSingleNode("/html/body/h1").InnerText;
            string scienceName = h1Inner.Replace(bird.Name, string.Empty).TrimStart(' ').TrimStart('\r').TrimStart('\n').TrimEnd(' ');
            bird.ScientificName = scienceName;
        }
        
        //We Return listItems because we use it when creating Identification Tips
        static List<string> AssignLengths(Bird bird, HtmlDocument document)
        {
            //Length & WingSpan & Descriptions
            string ulText = document.DocumentNode.SelectSingleNode("/html/body/ul[1]").InnerHtml;
            List<string> listItems = ulText.Split('<').ToList();

            //Length & WingSpan Only
            string length = "";
            string wingspan = "";
            string sizes = listItems[1];
            bool foundLength = false;
            for (int i = 0; i < sizes.Length; i++)
            {
                if (IsANumber(sizes[i]))
                {
                    if (!foundLength)
                    {
                        foundLength = true;
                        while (IsANumber(sizes[i]))
                        {
                            length += sizes[i];
                            i++;
                        }
                    }
                    else
                    {
                        while (IsANumber(sizes[i]))
                        {
                            wingspan += sizes[i];
                            i++;
                        }
                        break;
                    }
                }
            }

            bird.Length = ConvertString(length);
            bird.WingSpan = ConvertString(wingspan);
            return listItems;
        }

        static void AssignIdText(Bird bird, HtmlDocument document, List<string> listItems)
        {
            //Identification Tips Here; Skip the first two
            string next = string.Empty;
            for (int i = 2; i < listItems.Count; i++)
            {
                //The unwanted items look like this: "/li>" so we can weed them out with a length check
                if (listItems[i].Length > 7)
                {
                    next = listItems[i]
                        .Replace("li>", string.Empty)
                        .Replace(System.Environment.NewLine, string.Empty)
                        .TrimStart(' ')
                        .TrimEnd(' ');
                    bird.IdentificationTips.Add(next);
                }
            }

            //Morph 1
            var morph1 = document.DocumentNode.SelectSingleNode("/html/body/ul[2]");
            if (morph1 != null)
            {
                string morph1Text = morph1.InnerHtml;
                List<string> morph1Descriptions = morph1Text.Split('<').ToList();

                string nextMorph1 = string.Empty;
                for (int i = 0; i < morph1Descriptions.Count; i++)
                {
                    //The unwanted items look like this: "/li>" so we can weed them out with a length check
                    if (morph1Descriptions[i].Length > 7)
                    {
                        nextMorph1 = morph1Descriptions[i]
                            .Replace("li>", string.Empty)
                            .Replace(System.Environment.NewLine, string.Empty)
                            .TrimStart(' ')
                            .TrimEnd(' ');
                        bird.MorphOne.Add(nextMorph1);
                    }
                }
            }

            //Morph 2
            var morph2 = document.DocumentNode.SelectSingleNode("/html/body/ul[3]");
            if (morph2 != null)
            {
                string morph2Text = morph2.InnerHtml;
                List<string> morph2Descriptions = morph2Text.Split('<').ToList();

                string nextMorph2 = string.Empty;
                for (int i = 0; i < morph2Descriptions.Count; i++)
                {
                    //The unwanted items look like this: "/li>" so we can weed them out with a length check
                    if (morph2Descriptions[i].Length > 7)
                    {
                        nextMorph2 = morph2Descriptions[i]
                            .Replace("li>", string.Empty)
                            .Replace(System.Environment.NewLine, string.Empty)
                            .TrimStart(' ')
                            .TrimEnd(' ');
                        bird.MorphTwo.Add(nextMorph2);
                    }
                }

            }
        }

        static void AssignColors(Bird bird, HtmlDocument document)
        {
            //Colors
            foreach (string s in bird.IdentificationTips)
            {
                var allWords = s.Split(' ', '-');
                foreach (string w in allWords)
                {
                    foreach (string color in Bird.PossibleColors)
                    {
                        if (w == color)
                        {
                            bird.Colors.Add(color.ToLower());
                        }
                    }
                }
            }
            foreach (string s in bird.MorphOne)
            {
                var allWords = s.Split(' ', '-');
                foreach (string w in allWords)
                {
                    foreach (string color in Bird.PossibleColors)
                    {
                        if (w == color)
                        {
                            bird.Colors.Add(color.ToLower());
                        }
                    }
                }
            }

            foreach (string s in bird.MorphTwo)
            {
                var allWords = s.Split(' ', '-');
                foreach (string w in allWords)
                {
                    foreach (string color in Bird.PossibleColors)
                    {
                        if (w == color)
                        {
                            bird.Colors.Add(color.ToLower());
                        }
                    }
                }
            }
        }

        static void AssignImages(Bird bird, HtmlDocument document)
        {
            using (WebClient client = new WebClient())
            {
                string rawImageDescriptions = document.DocumentNode.SelectSingleNode("/html/body").InnerText;
                string[] parsedDescriptions = rawImageDescriptions.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var imageUrls = document.DocumentNode.SelectSingleNode("/html/body").Elements("a");
                int count = 0;
                string url = string.Empty;
                foreach (var anchor in imageUrls)
                {
                    BirdImage birdImage = new BirdImage();
                    url = anchor.GetAttributeValue("href", null);
                    url = url.TrimStart(' ', '.').Insert(0, _framesUrl);
                    try
                    {
                        birdImage.Image = client.DownloadData(url);
                    }
                    catch (WebException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine($"Failed url: {url}");
                        throw e;
                    }
                    
                    birdImage.Description = parsedDescriptions[count];
                    bird.Images.Add(birdImage);
                    count++;
                }
            }
        }

        //**HELPER METHODS
        static List<string[]> GetTopLevelData(string url)
        {
            //results will contain an array of 3 strings, order, family and frame url
            List<string[]> results = new List<string[]>();
            HtmlWeb web = _web;
            //This is early enough in the process don't worry about try/catch
            HtmlDocument document = web.Load(url);
            HtmlNodeCollection orders = new HtmlNodeCollection(document.DocumentNode);
            
            for (int i = 1; i < 23; i++)
            {
                HtmlNode orderNode = document.DocumentNode.SelectSingleNode(TopLevelHelper(i));
                string orderName = orderNode.FirstChild.InnerText;
                orderName = ScrubFamilyAndOrder(orderName);

                //Ordering of nodes goes a, #text, ul, #text
                var familyNodes = orderNode.SelectSingleNode("ul").SelectNodes("li");
                foreach (var family in familyNodes)
                {
                    string familyName = family.FirstChild.InnerText;
                    familyName = ScrubFamilyAndOrder(familyName);

                    //These should be the li's that contain the frame links
                    var frameNodes = family.SelectSingleNode("ul").SelectNodes("li");
                    foreach (var frame in frameNodes)
                    {
                        //The array will eventually hold the content frame url at 2, and the images frame at 3
                        string[] result = new string[4];
                        var anchor = frame.FirstChild.ChildAttributes("href");
                        string frameUrl = string.Empty;
                        //This isn't really a collection, it will only have one anchor element
                        foreach (var a in anchor)
                        {
                            frameUrl = _domain + a.Value;
                        }
                        result[0] = orderName;
                        result[1] = familyName;
                        result[2] = frameUrl;
                    }
                    Console.WriteLine("");
                }
            }
            return results;
        }

        static string TopLevelHelper(int n)
        {
            return $"/html/body/ul[2]/li[{n}]";
        }
        //remove everything but the url
        static string ScrubFamilyAndOrder(string data)
        {
            return String.Concat(data.SkipWhile(c => c != ':')).TrimStart(':').TrimStart(' ').TrimEnd(' ').TrimEnd('\n').TrimEnd('\r');
        }

        //This gets the urls inside the frames. Each entry in results will now have the content and images frame urls
        static List<string[]> GetContentFrameURLs(List<string[]> data)
        {
            HtmlWeb web = _web;
            HtmlDocument document;
            string result = string.Empty;

            for(int i = 0; i < data.Count; i++)
            {
                try
                {
                    //GetTopLevelData() assigns the top level frame url to [2], we'll overwrite that below to be the content frame url
                    document = web.Load(data[i][2]);
                    var allAttributes = document.DocumentNode.Descendants("frame").Select(y => y.Attributes);
                    int count = 0;
                    foreach (var frameGroup in allAttributes)
                    {
                        if (count == 1)
                        {
                            foreach (var attribute in frameGroup)
                            {
                                if (attribute.Name == "src" || attribute.Name == "SRC")
                                {
                                    string contentFrameUrl = _domain + attribute.Value;
                                    data[i][2] = contentFrameUrl;
                                    //add breaks we're done
                                }
                            }
                        }

                        if (count == 2)
                        {
                            foreach (var attribute in frameGroup)
                            {
                                if (attribute.Name == "src" || attribute.Name == "SRC")
                                {
                                    string imagesFrameUrl = _domain + attribute.Value;
                                    data[i][3] = imagesFrameUrl;
                                }
                            }
                        }
                        count++;
                    }
                }
                catch (WebException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine($"Failed url: {data[i][2]}");
                    throw e;
                }
            }
            return data;
        }

        static bool IsANumber(char c)
        {
            return c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9';
        }

        static int ConvertString(string s)
        {
            int result = 0;
            int multiplier = 1;
            for(int i = s.Length - 1; i >= 0; i--)
            {
                int nextDigit = s[i] - 48;
                result += nextDigit * multiplier;
                multiplier *= 10;
            }

            return result;
        }

        static void Main(string[] args)
        {
            //string currentDirectory = Directory.GetCurrentDirectory();
            //DirectoryInfo directory = new DirectoryInfo(currentDirectory);
            //var fileName = Path.Combine(directory.FullName, "FirstRun.txt");
            //var file = new FileInfo(fileName);
            //Run(file.FullName);


            string content = "https://www.mbr-pwrc.usgs.gov/id/framlst/Idtips/h1620id.html";
            string images = "https://www.mbr-pwrc.usgs.gov/id/framlst/photo_htm/p1620.html";
            List<string[]> test = new List<string[]> { new string[] { "Order Filler", "Family Filler", content, images} };
            //GetContentFrameURLs(test);
            CreateBird(test);
            
            //GetTopLevelData(_framesUrl);
        }
    }
}
