namespace SapphireDb.Models.SapphireApiBuilder
{
    public interface ISapphireModelConfiguration
    {
    }
    
    public interface ISapphireModelConfiguration<T> : ISapphireModelConfiguration
    {
        public void Configure(SapphireModelBuilder<T> modelBuilder);
    }
}