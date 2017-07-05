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
        static Dictionary<Family, Order> OrdersByFamily = new Dictionary<Family, Order>();

        const string _framesUrl = "https://www.mbr-pwrc.usgs.gov/id/framlst";
        const string _domain = "https://www.mbr-pwrc.usgs.gov";
        
        static void Run(string url)
        {
            List<string[]> topLevelData = GetTopLevelData(url);
            List<string[]> frameUrls = GetContentFrameURLs(topLevelData);
            CreateBirds(frameUrls);
        }

        //Create Bird Classes
        //Order of string[]: order and family names, content and images urls respectively
        static void CreateBirds(List<string[]> urls)
        {
            foreach (string[] dataBundle in urls)
            {
                Bird bird = new Bird();
                //Assign Order and Family Navigation Properties
                if (!Orders.ContainsKey(dataBundle[0]))
                {
                    Order order = new Order(dataBundle[0]);
                    Orders.Add(dataBundle[0], order);
                    //We know we need a new family because orders contain unique families
                    Family family = new Family(dataBundle[1], order);
                    family.OrderId = order.Id;
                    bird.Family = family;
                    Families.Add(dataBundle[1], family);
                }
                else if (!Families.ContainsKey(dataBundle[1]))
                {
                    Family family = new Family(dataBundle[1], Orders[dataBundle[0]]);
                    family.OrderId = Orders[dataBundle[0]].Id;
                    bird.Family = family;
                    bird.FamilyId = family.Id;
                    Families.Add(dataBundle[1], family);
                }
                //Assign the Id property because the logic in Repository that avoids duplicate entries requires it
                else
                {
                    bird.FamilyId = Families[dataBundle[1]].Id;
                    bird.Family = Families[dataBundle[1]];
                    bird.Family.OrderId = Orders[dataBundle[0]].Id;
                    bird.Family.Order = Orders[dataBundle[0]];
                }
                
                HtmlWeb web = _web;
                //Here we load the data from the content frame
                try
                {
                    HtmlDocument document = new HtmlDocument();
                    document = web.Load(dataBundle[2]);

                    AssignName(bird, document);
                    AssignScientificName(bird, document);
                    //Assigning Lengths and the first Description object use the same strings found in descripText
                    List<string> descripText = AssignLengths(bird, document);
                    AssignDescriptions(bird, document, descripText);
                    AssignColorsAndHabitat(bird);
                }
                catch (WebException e)
                {
                    Console.WriteLine($"Exception Message: {e.Message}");
                    Console.WriteLine($"Remote Host Response Status: {e.Status}");
                    Console.WriteLine($"Remote Host Response: {e.Response}");
                    Console.WriteLine($"Failed to load content frame: {dataBundle[2]}");
                    Console.ReadLine();
                    throw e;
                }
                try
                {
                    //Grab images from images frame
                    HtmlDocument document = new HtmlDocument();
                    document = web.Load(dataBundle[3]);

                    AssignImages(bird, document);
                    Repository.AddBird(bird);
                }
                catch (WebException e)
                {
                    Console.WriteLine($"Exception Message: {e.Message}");
                    Console.WriteLine($"Remote Host Response Status: {e.Status}");
                    Console.WriteLine($"Remote Host Response: {e.Response}");
                    Console.WriteLine($"Failed to load content frame: {dataBundle[3]}");
                    Console.ReadLine();
                    throw e;
                }
            }
        }

        //***BIRD METHODS***
        ///html/head/title
        static void AssignName(Bird bird, HtmlDocument document)
        {
            string innerTitle = document.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
            bird.Name = innerTitle.Replace("Identification tips", string.Empty).TrimEnd(' ');
        }

        static void AssignScientificName(Bird bird, HtmlDocument document)
        {
            string h1Inner = document.DocumentNode.SelectSingleNode("/html/body/h1").InnerText;
            bird.ScientificName = h1Inner.Replace(bird.Name, string.Empty).TrimStart(' ', '\r', '\n').TrimEnd(' ');
        }
        
        //We Return listItems because we use it when creating Identification Tips
        static List<string> AssignLengths(Bird bird, HtmlDocument document)
        {
            string idText = document.DocumentNode.SelectSingleNode("/html/body/ul[1]").InnerHtml;
            List<string> primaryIdTips = idText.Split('<').ToList();
            string length = string.Empty;
            string wingspan = string.Empty;
            //sizes contains the length and wingspan
            string sizes = primaryIdTips[1];
            bool foundLength = false;

            for (int i = 0; i < sizes.Length; i++)
            {
                //length always comes first
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
                        //Increment i to avoid a decimal value breaking the logic
                        i += 3;
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
            return primaryIdTips;
        }
        //Import primaryIdTips from AssignLengths to avoid redundant work
        static void AssignDescriptions(Bird bird, HtmlDocument document, List<string> primaryIdTips)
        {
            //The descriptions in primaryIdTips are unqiue because we don't want the first two so I don't use AssignDescripionHelper()
            string next = string.Empty;
            Description primary = new Description(bird, "Primary");
            bird.Descriptions.Add(primary);
            for (int i = 2; i < primaryIdTips.Count; i++)
            {
                next = ScrubDescriptions(primaryIdTips[i]);
                if (next != null)
                {
                    BulletPoint bullet = new BulletPoint(next, primary);
                    primary.BulletPoints.Add(bullet);
                }
            }
            //Assign Secondary Descriptions
            int ul = 2;
            while (AssignDescriptionHelper(bird, document, ul))
            {
                ul++;
            }
            //Assign SimilarSpecies
            var h3Nodes = document.DocumentNode.SelectSingleNode("/html/body").Elements("h3");
            foreach (var h3 in h3Nodes)
            {
                if (h3.InnerText.Contains("Similar"))
                {
                    //Some pages are structured differently so we need to do this check
                    if (h3.NextSibling.NextSibling != null && h3.NextSibling.NextSibling.InnerText.Length > 5)
                    {
                        bird.SimilarSpecies = h3.NextSibling.NextSibling.InnerText.Replace(System.Environment.NewLine, string.Empty).TrimStart(' ');
                    }
                    else
                    {
                        bird.SimilarSpecies = h3.NextSibling.InnerText.Replace(System.Environment.NewLine, string.Empty).TrimStart(' ');
                    }
                }
            }
        }

        static void AssignColorsAndHabitat(Bird bird)
        {
            HashSet<string> colorsFound = new HashSet<string>();
            //We check if this is a seabird only at Descriptions[0]
            for (int i = 0; i < bird.Descriptions.Count; i++)
            {
                if (i == 0)
                {
                    AssignColorsAndHabitatHelper(bird, bird.Descriptions[i].BulletPoints, colorsFound, true);
                }
                else
                {
                    AssignColorsAndHabitatHelper(bird, bird.Descriptions[i].BulletPoints, colorsFound, false);
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
                    BirdImage birdImage = new BirdImage(bird);
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

        //**HELPER METHODS BELOW**
        static List<string[]> GetTopLevelData(string url)
        {
            //results will contain an array of 3 strings, order, family and frame url
            List<string[]> topLevelData = new List<string[]>();
            HtmlWeb web = _web;
            //This is early in the process so I didn't include exception handling
            HtmlDocument document = web.Load(url);
            
            //There are 22 orders of birds listed so i = 23 is production value
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

                    //These should be the li's that contain the top level frame links
                    var frameNodes = family.SelectSingleNode("ul").SelectNodes("li");
                    foreach (var frame in frameNodes)
                    {
                        //The array will eventually hold 4 strings, order, family, content frame url, images frame url
                        string[] result = new string[4];
                        var anchor = frame.FirstChild.ChildAttributes("href");
                        string frameUrl;
                        //This isn't really a collection, it will only have one anchor element
                        foreach (var a in anchor)
                        {
                            frameUrl = _framesUrl + "/" + a.Value;
                            //We'll also grab the order and family of the bird here because it's an opportune time
                            result[0] = orderName;
                            result[1] = familyName;
                            result[2] = frameUrl;
                            topLevelData.Add(result);
                        }
                    }
                }
            }
            return topLevelData;
        }
        //Return an XPath with the next li to crawl
        static string TopLevelHelper(int n)
        {
            return $"/html/body/ul[2]/li[{n}]";
        }
        //remove everything but the url
        static string ScrubFamilyAndOrder(string data)
        {
            return String.Concat(data.SkipWhile(c => c != ':')).TrimStart(':', ' ').TrimEnd(' ', '\n', '\r');
        }

        static string ScrubDescriptions(string listItem)
        {
            //The unwanted items appear to always be shorter than 7 characters so we can weed them out with a length check
            if (listItem.Length > 6)
            {
                return listItem
                    .Replace("li>", string.Empty)
                    .Replace(System.Environment.NewLine, string.Empty)
                    .TrimStart(' ')
                    .TrimEnd(' ');
            }
            else return null;
        }

        static bool AssignDescriptionHelper(Bird bird, HtmlDocument document, int ul)
        {
            //Grap the h3 and assign as the Title of this SecondaryDescription. The h3 will always have an xpath [ul - 1]
            var headingNode = document.DocumentNode.SelectSingleNode($"/html/body/h3[{ul - 1}]");
            //This node contains all the bullet point descriptions
            var descriptionNode = document.DocumentNode.SelectSingleNode($"/html/body/ul[{ul}]");
            string next = string.Empty;
            if (descriptionNode != null)
            {
                string title = headingNode.InnerText.TrimEnd(' ', ':');
                Description secondary = new Description(bird, title);
                bird.Descriptions.Add(secondary);
                
                string descriptionText = descriptionNode.InnerHtml;
                List<string> tempDescriptions = descriptionText.Split('<').ToList();

                for (int i = 0; i < tempDescriptions.Count; i++)
                { 
                    next = ScrubDescriptions(tempDescriptions[i]);
                    if (next != null)
                    {
                        BulletPoint bullet = new BulletPoint(next, secondary);
                        secondary.BulletPoints.Add(bullet);
                    }
                }
                return true;
            }
            return false;
        }

        static void AssignColorsAndHabitatHelper(Bird bird, List<BulletPoint> descriptions, HashSet<string> colorsFound, bool seabirdCheck)
        {
            if (!seabirdCheck)
            {
                foreach (BulletPoint b in descriptions)
                {
                    string[] allWords = b.Text.Split(' ', '-');
                    foreach (string w in allWords)
                    {
                        string next = w.ToLower();
                        foreach (string possibleColor in Bird.PossibleColors)
                        {
                            //I need an exact match to avoid false positives, like picking up red from Fred
                            if (next == possibleColor)
                            {
                                if (!colorsFound.Contains(next))
                                {
                                    Color color = new Color(bird, next);
                                    bird.Colors.Add(color);
                                    colorsFound.Add(next);
                                }
                            }
                        }
                    }
                }
            }
            //Determine if this is a seabird along with color checking
            else
            {
                foreach (BulletPoint b in descriptions)
                {
                    string[] allWords = b.Text.Split(' ', '-');
                    foreach (string w in allWords)
                    {
                        string next = w.ToLower();
                        foreach (string possibleColor in Bird.PossibleColors)
                        {
                            //I need an exact match to avoid false positives, like picking up red from Fred
                            if (next == possibleColor)
                            {
                                if (!colorsFound.Contains(next))
                                {
                                    Color color = new Color(bird, next);
                                    bird.Colors.Add(color);
                                    colorsFound.Add(next);
                                }
                            }
                            if (next == "pelagic" || next == "seafaring")
                            {
                                bird.Pelagic = true;
                            }
                        }
                    }
                }
            }
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
                    //GetTopLevelData() assigns the top level frame url to [2], we'll overwrite that below to be the contentFrameUrl
                    //We skip the first frameGroup because it contains only useless navigation links
                    document = web.Load(data[i][2]);
                    var srcAttribute = document.DocumentNode.SelectSingleNode("/html/frameset/frame[2]").ChildAttributes("src");
                    //Should be only one match so this isn't a collection
                    foreach (var attribute in srcAttribute)
                    {
                        string contentFrameUrl = _domain + attribute.Value;
                        data[i][2] = contentFrameUrl;
                    }
                    srcAttribute = document.DocumentNode.SelectSingleNode("/html/frameset/frame[3]").ChildAttributes("src");
                    //Should be only one match so this isn't a collection
                    foreach (var attribute in srcAttribute)
                    {
                        string imagesFrameUrl = _domain + attribute.Value;
                        data[i][3] = imagesFrameUrl;
                    }
                }
                catch (WebException e)
                {
                    Console.WriteLine($"Exception Message: {e.Message}");
                    Console.WriteLine($"Remote Host Response Status: {e.Status}");
                    Console.WriteLine($"Remote Host Response: {e.Response}");
                    Console.WriteLine($"Failed url: {data[i][2]}");
                    Console.ReadLine();
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

        public static void Test(string url)
        {
            // /html/body/text()
            // /html/body/h1
            //  /html/body/h1
            HtmlWeb web = _web;
            HtmlDocument document = new HtmlDocument();
            document = web.Load(url);
            var h3s = document.DocumentNode.SelectSingleNode("/html/body").Elements("h3");
            foreach (var h3 in h3s)
            {
                if (h3.InnerText.Contains("Similar"))
                {
                    //Some pages are structured differently so we need to do this check
                    if (h3.NextSibling.NextSibling != null && h3.NextSibling.NextSibling.InnerText.Length > 5)
                    {
                        string s = h3.NextSibling.NextSibling.InnerText.Replace(System.Environment.NewLine, string.Empty);
                    }
                    else
                    {
                        string s = h3.NextSibling.InnerText.Replace(System.Environment.NewLine, string.Empty);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            //**TESTING CODE COMMENTED OUT BELOW**
            //string content = "https://www.mbr-pwrc.usgs.gov/id/framlst/Idtips/h0350id.html";
            //string content1 = "https://www.mbr-pwrc.usgs.gov/id/framlst/Idtips/h2721id.html";
            //string images = "https://www.mbr-pwrc.usgs.gov/id/framlst/photo_htm/p1620.html";
            //List<string[]> test = new List<string[]> { new string[] { "Order Filler", "Family Filler", content, images} };
            //GetContentFrameURLs(test);
            //CreateBirds(test);
            //Test(content1);

            Run(_framesUrl);
        }
    }
}