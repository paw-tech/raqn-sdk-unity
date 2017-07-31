using FullSerializer;

namespace RAQN
{
    [fsObject]
    public class RaqnUser
    {
        public const string TYPE_LEARNER = "LEARNER";
        public const string TYPE_EDUCATOR = "EDUCATOR";
        public const string TYPE_INSTITUTION = "INSTITUTION";
        public const string TYPE_PUBLISER = "PUBLISHER";

        [fsProperty]
        public string id;
        [fsProperty]
        public string email;
        [fsProperty]
        public string type;

        [fsProperty]
        public RaqnUserProfile profile;
    }

    [fsObject]
    public struct RaqnProfileName {
        [fsProperty]
        public string first;
        
        [fsProperty]
        public string last;
    }

    [fsObject]
    public class RaqnUserProfile
    {
        [fsProperty]
        public RaqnProfileName name;
        [fsProperty]
        public string avatar;

        [fsProperty]
        public string nickname;

        [fsProperty]
        public string gender;
        [fsProperty]
        public string country;
         [fsProperty]
        public string birthday;
        [fsProperty]
        public string language;

    }
}