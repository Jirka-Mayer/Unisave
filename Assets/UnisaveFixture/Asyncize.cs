using System;
using System.Collections;
using System.Threading.Tasks;
using Unisave.Facets;

namespace UnisaveFixture
{
    /// <summary>
    /// Makes IEnumerator tests asynchronous for easier unisave testing
    /// </summary>
    public static class Asyncize
    {
        public static IEnumerator UnityTest(Func<Task> body)
        {
            async Task<object> Wrapper()
            {
                await body.Invoke();
                return null;
            }
            
            yield return new FacetCall(null, Wrapper());
        }
    }
}