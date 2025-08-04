using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Core
{
    public static class TokenManager
    {
        private static readonly List<CancellationTokenSource> _sources = new();

        // Registriert einen neuen TokenSource, gibt den Token zurück
        public static CancellationToken CreateToken()
        {
            var cts = new CancellationTokenSource();
            _sources.Add(cts);
            return cts.Token;
        }

        // Optional: Gibt alle TokenSources zurück (für gezieltes Canceln)
        public static IReadOnlyList<CancellationTokenSource> Sources => _sources.AsReadOnly();

        // Bricht alle Token ab und leert die Liste
        public static void CancelAll()
        {
            foreach (var cts in _sources)
                cts.Cancel();
            _sources.Clear();
        }
    }

}
