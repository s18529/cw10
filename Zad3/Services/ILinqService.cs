using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zad3.Helpers;
using Zad3.NewModels;

namespace Zad3.Services
{
    public interface ILinqService
    {
        public IEnumerable<Student> listStudent();
        public MyHelper updateStudent(Student student);
        public MyHelper deleteSteudent(string id);
        public MyHelper addStudent(Models.Student student);
        public MyHelper promote(Enrollment enrollment);

    }
}
