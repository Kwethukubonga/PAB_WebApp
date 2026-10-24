using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PhilisaAbantuBethu.Data.Protection;

/// <summary>
/// Entity Framework builds the model once per context type and reuses it. The model here holds the
/// encrypting converter of whichever <see cref="ISensitiveText"/> built it first, so a second
/// context with different keys would quietly be encrypted and decrypted with the first one's.
/// Keying the cached model on that instance keeps each one's keys its own. The running site has a
/// single instance, so it still builds the model once; the difference is only felt where several
/// sites with different keys share a process, as the tests do.
/// </summary>
internal sealed class SensitiveModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime) =>
        (context.GetType(), (context as AppDbContext)?.Sensitive, designTime);
}
