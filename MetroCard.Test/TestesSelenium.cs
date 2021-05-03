using OpenQA.Selenium.Chrome;
using Xunit;

namespace MetroCard.Test
{
    public class Selenium
    {
        [Theory]
        [InlineData("teste@teste.com", "Teste@123")]
        public void RealizarLogin(string email, string senha)
        {

            var driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://localhost:44357/Identity/Account/Login");


            var campoEmail = driver.FindElementById("Input_Email");
            campoEmail.SendKeys(email);

            var campoSenha = driver.FindElementById("Input_Password");
            campoSenha.SendKeys(senha);

            driver.FindElementByTagName("form").Submit();
        }
    }
}
