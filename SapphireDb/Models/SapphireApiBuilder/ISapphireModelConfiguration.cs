namespace SapphireDb.Models.SapphireApiBuilder
{
    public interface ISapphireModelConfiguration
    {
    }
    
    public interface ISapphireModelConfiguration<T> : ISapphireModelConfiguration
        where T : class
    {
        public void Configure(SapphireModelBuilder<T> modelBuilder);
    }
}