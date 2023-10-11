using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.ValueConversion;

public interface IKdbndpArrayConverter
{
    ValueConverter ElementConverter { get; }
}
