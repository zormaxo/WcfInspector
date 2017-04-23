using System.ServiceModel;
using System.ServiceModel.Web;

namespace WcfIsnpectorService
{
    [ServiceContract]
    public interface IStudentOperationService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "RestStu")]
        Student GetStudent(Student student);
    }
}