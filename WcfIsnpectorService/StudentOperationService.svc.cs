namespace WcfIsnpectorService
{
    public class StudentOperationService : IStudentOperationService
    {
        public Student GetStudent(Student student)
        {
            return new Student
            {
                Name = "Ekrem"
            };
        }
    }
}