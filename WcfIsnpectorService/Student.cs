using System.Runtime.Serialization;

namespace WcfIsnpectorService
{
    [DataContract(Namespace = "")]
    public class Student
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Age { get; set; }
    }
}