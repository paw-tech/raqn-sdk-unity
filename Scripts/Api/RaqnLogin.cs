using FullSerializer;
using RAQN.Serialization;
using RAQN.Storage;

namespace RAQN.Api
{
    [fsObject]
    public class RaqnLogin
    {
        public string token;
        public RaqnUser user;
    }
}