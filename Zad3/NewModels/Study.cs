using System;
using System.Collections.Generic;

#nullable disable

namespace Zad3.NewModels
{
    public partial class Study
    {
        public Study()
        {
            Enrollments = new HashSet<Enrollment>();
        }

        public int IdStudy { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
