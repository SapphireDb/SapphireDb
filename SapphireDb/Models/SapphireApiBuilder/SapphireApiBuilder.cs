namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireApiBuilder
    {
        public SapphireModelBuilder<T> Model<T>()
        {
            return new SapphireModelBuilder<T>();
        }
    }
}