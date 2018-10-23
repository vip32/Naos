namespace Naos.Core.Domain
{
    public interface IDiscriminatedEntity
    {
        string Discriminator { get; }
    }
}
