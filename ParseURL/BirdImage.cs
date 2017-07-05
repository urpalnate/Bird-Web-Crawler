namespace ParseURL
{
    //Dependent Entity in relation to Bird
    public class BirdImage
    {
        public BirdImage(Bird bird)
        {
            Bird = bird;
        }
        //Primary and Foreign Keys
        public int Id { get; set; }
        public int BirdId { get; set; }

        //Scalar Properties
        public byte[] Image { get; set; }
        public string Description { get; set; }

        //Navigation Property
        public virtual Bird Bird { get; set; }
    }
}
