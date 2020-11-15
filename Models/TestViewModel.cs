using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Html;

namespace EditorForProject.Models
{
    public enum Options
    {
        Option1,
        Option2,
        Option3
    }
    public class TestViewModel
    {
        public string str { get; set; }
        [Range(1, 10)]
        [Required]
        public int i { get; set; }
        public long @long { get; set; }
        public bool @bool { get; set; }
        public Options @enum { get; set; }
        public Test2 test2 { get; set; }
        public Test3 outerTest3 { get; set; }
    }

    public class Test2
    {
        [DisplayName("Test 2 string")]
        [Required(ErrorMessage = "Это поле является обязательным.")]
        public string test2str { get; set; }

        public int test2int { get; set; } = 123;
        
        public Test3 test3 { get; set; }
    }

    public class Test3
    {
        public string test3str = "test3 string";
        public Options test3enum;
        public TestViewModel ViewModel;
    }

    public class Test4
    {
        [DisplayName("Test 4 string")]
        public string str { get; set; }
        [Range(1, 10)]
        [Required]
        public int i { get; set; }
        public long @long { get; set; }
        public bool @bool { get; set; }
    }
}