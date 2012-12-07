using System;

namespace CannedBytes.Media.IO.Services
{
    public interface IChunkTypeFactory
    {
        object CreateChunkObject(FourCharacterCode chunkTypeId);

        Type LookupChunkObjectType(FourCharacterCode chunkTypeId);
    }
}