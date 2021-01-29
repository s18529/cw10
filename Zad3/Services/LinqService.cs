using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Zad3.Helpers;
using Zad3.NewModels;

namespace Zad3.Services
{
    public class LinqService : ILinqService
    {
        readonly s18529Context context;
        public LinqService(s18529Context _context)
        {
            context = _context;
        }

        public MyHelper addStudent(Models.Student student)
        {
            try
            {
                var count = context.Enrollments.Join(context.Studies, enroll => enroll.IdStudy, stud => stud.IdStudy, (enroll, stud) => new { enroll, stud }).Where(x => x.stud.Name == student.Studies && x.enroll.Semester == 1).Count();
                if (count > 0)
                {
                    var enroll = context.Enrollments.Join(context.Studies, enroll => enroll.IdStudy, stud => stud.IdStudy, (enroll, stud) => new { enroll, stud }).Where(stud => stud.stud.Name == student.Studies && stud.enroll.Semester == 1).Select(x => x.enroll.IdEnrollment).First();
                    var nowyStudent = new Student();
                    nowyStudent.FirstName = student.FirstName;
                    nowyStudent.LastName = student.LastName;
                    nowyStudent.BirthDate = student.Bdate;
                    nowyStudent.IndexNumber = student.IndexNumber;
                    byte[] random = new byte[128 / 8];
                    string salt;
                    using (var generator = RandomNumberGenerator.Create())
                    {
                        generator.GetBytes(random);
                        salt = Convert.ToBase64String(random);
                    }
                    var valueBytes = KeyDerivation.Pbkdf2(
                                password: student.Password,
                                salt: Encoding.UTF8.GetBytes(salt),
                                prf: KeyDerivationPrf.HMACSHA512,
                                iterationCount: 10000,
                                numBytesRequested: 256 / 8);
                    nowyStudent.Password = Convert.ToBase64String(valueBytes);
                    nowyStudent.Salt = salt;
                    nowyStudent.IdEnrollment = enroll;
                    context.Students.Add(nowyStudent);
                    var enrollment = context.Enrollments.Where(x => x.IdEnrollment == enroll).First();
                    enrollment.Students.Add(nowyStudent);
                    context.SaveChanges();
                    MyHelper helper = new MyHelper("added", 0);
                    helper.enrollment = enrollment;
                    return helper;
                }
                else
                {
                    var idStudy = context.Studies.Where(x => x.Name == student.Studies).Select(x => x.IdStudy).First();
                    HashSet<Student> set = new HashSet<Student>();
                    var enrollment = new Enrollment
                    {
                        Semester = 1,
                        IdStudy = idStudy,
                        StartDate = DateTime.Now,
                        Students = set
                    };
                    context.Enrollments.Add(enrollment);
                    context.SaveChanges();
                    var nowyStudent = new Student();
                    nowyStudent.FirstName = student.FirstName;
                    nowyStudent.LastName = student.LastName;
                    nowyStudent.BirthDate = student.Bdate;
                    nowyStudent.IndexNumber = student.IndexNumber;
                    byte[] random = new byte[128 / 8];
                    string salt;
                    using (var generator = RandomNumberGenerator.Create())
                    {
                        generator.GetBytes(random);
                        salt = Convert.ToBase64String(random);
                    }
                    var valueBytes = KeyDerivation.Pbkdf2(
                                password: student.Password,
                                salt: Encoding.UTF8.GetBytes(salt),
                                prf: KeyDerivationPrf.HMACSHA512,
                                iterationCount: 10000,
                                numBytesRequested: 256 / 8);
                    nowyStudent.Password = Convert.ToBase64String(valueBytes);
                    nowyStudent.Salt = salt;
                    nowyStudent.IdEnrollment = enrollment.IdEnrollment;
                    context.Students.Add(nowyStudent);
                    enrollment.Students.Add(nowyStudent);
                    context.SaveChanges();
                    MyHelper helper = new MyHelper("added", 0);
                    helper.enrollment = enrollment;
                    return helper;
                }
            }
            catch (Exception ex)
            {
                return new MyHelper(ex.ToString(), -1);
            }

        }

        public MyHelper deleteSteudent(string id)
        {
            try 
            {
                var student = context.Students.Where(x => x.IndexNumber == id).First();
                context.Students.Remove(student);
                context.SaveChanges();
            }catch(Exception ex)
            {
                return new MyHelper(ex.ToString(), -1);
            }
            return new MyHelper("deleted", 0);
            
        }

        public IEnumerable<Student> listStudent()
        {
             return context.Students.ToList();
        }

        public MyHelper promote(Enrollment enrollment)
        {
            try
            {
                var res = context.Students.Where(x => x.IdEnrollment == enrollment.IdEnrollment);
                var count = context.Enrollments.Where(x => x.Semester == (enrollment.Semester + 1) && x.IdStudy == enrollment.IdStudy).Count();
                if (count > 0)
                {
                    var enroll = context.Enrollments.Where(x => x.Semester == (enrollment.Semester + 1) && x.IdStudy == enrollment.IdStudy).First();
                    foreach (var s in res)
                    {
                        var idold = s.IdEnrollment;
                        s.IdEnrollment = enroll.IdEnrollment;
                        enroll.Students.Add(s);
                        var oldEnroll = context.Enrollments.Where(x => x.IdEnrollment == idold).First();
                        oldEnroll.Students.Remove(s);
                    }
                    context.SaveChanges();
                    MyHelper helper = new MyHelper("promoted", 0);
                    helper.enrollment = enroll;
                    return helper;
                }
                else
                {
                    HashSet<Student> set = new HashSet<Student>();
                    var enrollmentNew = new Enrollment
                    {
                        Semester = enrollment.Semester + 1,
                        IdStudy = enrollment.IdStudy,
                        StartDate = DateTime.Now,
                        Students = set
                    };
                    context.Enrollments.Add(enrollmentNew);
                    context.SaveChanges();
                    foreach (var s in res)
                    {
                        var idold = s.IdEnrollment;
                        var oldEnroll = context.Enrollments.Where(x => x.IdEnrollment == idold).First();
                        s.IdEnrollment = enrollmentNew.IdEnrollment;
                        enrollmentNew.Students.Add(s);
                        oldEnroll.Students.Remove(s);
                    }
                    context.SaveChanges();
                    MyHelper helper = new MyHelper("promoted", 0);
                    helper.enrollment = enrollmentNew;
                    return helper;

                }
            }
            catch (Exception ex)
            {
                return new MyHelper(ex.ToString(), -1);
            }
        }

        public MyHelper updateStudent(Student student)
        {
            try
            {
                context.Students.Attach(student);
                context.Entry(student).State = EntityState.Modified;
                context.SaveChanges();
            }catch(Exception ex)
            {
                return new MyHelper(ex.ToString(), -1);
            }
            return new MyHelper("updated", 0);
        }
    }
}
