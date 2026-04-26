using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace WebFeedReader.Utils
{
    public static class EnumerableStreamer
    {
        public static async IAsyncEnumerable<List<T>> GetChunkyStream<T>(
            List<T> source,
            int initialChunkSize,
            int batchThreshold,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var chunkSize = initialChunkSize;

            for (var i = 0; i < source.Count;)
            {
                token.ThrowIfCancellationRequested();

                List<T> chunk;
                if (i >= batchThreshold)
                {
                    // 閾値を超えたら残り全部
                    chunk = source.Skip(i).ToList();
                    i = source.Count; // 終了フラグ
                }
                else
                {
                    // 序盤のチャンク処理
                    chunk = source.Skip(i).Take(chunkSize).ToList();
                    i += chunkSize;
                    chunkSize++; // 徐々に増やす
                }

                yield return chunk;

                if (i >= source.Count)
                {
                    break;
                }

                // 次の塊までのウェイト
                await Task.Delay(50, token);
            }
        }
    }
}