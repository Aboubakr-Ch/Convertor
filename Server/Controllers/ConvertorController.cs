using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvertorController : ControllerBase
    {
        private readonly ILogger<ConvertorController> _logger;
        private static string? finalResult { get; set; }

        [HttpGet]
        public string Get()
        {
            return finalResult;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult Post([FromBody] string value)
        {
            try
            {
                finalResult = ConvertToWords(value.ToCharArray());
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.UtcNow);
                _logger.LogInformation("Exception", ex.Message);
            }
            return NoContent();
        }

        private string ConvertToWords(char[] input)
        {
            string? firstPartResult = null;
            string? finalResult = null;
            string? sum = null;
            var space = " ";
            var singlesUnits = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensUnites = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            var found = input.Where(o => o.Equals('.')).ToString();
            for (int i = input.Length - 1; i >= 0; i--)
            {
                var separator = input[i];
                if (separator == '.')
                {
                    string part;
                    if (input.Length >= i + 2) part = "0";
                    else part = input[i + 2].ToString();

                    var separatorPart = input[i + 1].ToString() + part;
                    var value = int.Parse(separatorPart.ToString());
                    if (value < 19)
                    {
                        firstPartResult = singlesUnits[value];
                    }
                    else if (value >= 20)
                    {
                        int? firstPart = value / 10;
                        int? secoundPart = value - firstPart * 10;

                        firstPartResult += tensUnites[(int)firstPart] + space;
                        if (secoundPart > 0 && secoundPart <= 9) firstPartResult += singlesUnits[(int)secoundPart] + space;
                        firstPartResult += "cent";
                    }
                    sum = null;
                }
                else
                {
                    sum = input[i].ToString() + sum;
                }
            }
            if (sum != null)
            {
                var value = int.Parse(sum.ToString());
                for (var j = 1000000000; j >= 1; j /= 1000)
                {
                    int hundreds = 0, tens = 0, ones = 0;
                    int result = value / j;
                    if (result != 0)
                    {
                        hundreds = result / 100;
                        tens = (result - hundreds * 100) / 10;
                        ones = result - hundreds * 100 - tens * 10;

                        if (ones + tens * 10 < 20)
                        {
                            finalResult += singlesUnits[hundreds] + " hunderds and " + singlesUnits[ones + tens * 10];
                        }
                        else
                        {
                            finalResult += singlesUnits[hundreds] + " hunderds and " + tensUnites[tens] + space + singlesUnits[ones];
                        }

                        switch (j)
                        {
                            case 1000000000:
                                finalResult += " billion ";
                                break;
                            case 1000000:
                                finalResult += " million ";
                                break;
                            case 1000:
                                finalResult += " thousands ";
                                break;
                        }
                    }
                    value = value - ((hundreds * 100 + tens * 10 + ones) * j);
                }
            }

            if (string.IsNullOrEmpty(firstPartResult)) return finalResult;
            if (string.IsNullOrEmpty(finalResult)) return firstPartResult;
            return finalResult + space + "and " + firstPartResult;
        }
    }
}
