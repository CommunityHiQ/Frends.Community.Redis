using System.ComponentModel;
using System.Threading;
using Microsoft.CSharp; // You can remove this if you don't need dynamic type in .NET Standard frends Tasks

#pragma warning disable 1591

namespace Frends.Community.Redis
{
    public static class Redis
    {
        /// <summary>
        /// This is task
        /// Documentation: https://github.com/CommunityHiQ/Frends.Community.Redis
        /// </summary>
        /// <param name="input">What to repeat.</param>
        /// <param name="options">Define if repeated multiple times. </param>
        /// <param name="cancellationToken"></param>
        /// <returns>{string Replication} </returns>
        public static Result FirstTask(Parameters input, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            var repeats = new string[options.Amount];

            for (var i = 0; i < options.Amount; i++)
            {
                // It is good to check the cancellation token somewhere you spend lot of time, e.g. in loops.
                cancellationToken.ThrowIfCancellationRequested();

                repeats[i] = input.Message;
            }

            var output = new Result
            {
                Replication = string.Join(options.Delimiter, repeats)
            };

            return output;
        }
    }
}
