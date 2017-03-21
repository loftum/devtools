using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Pat.Models
{
    public class ResponseContent : PropertyChangeNotifier
    {
        public virtual string Body { get; set; }

        public async Task Update(HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                {
                    Body = "Stream is null. Wat.";
                    return;
                }
                using (var reader = new StreamReader(stream))
                {
                    var result = await reader.ReadToEndAsync();
                    Body = result;
                }
            }
        }
    }
}