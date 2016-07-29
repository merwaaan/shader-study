namespace Shaders
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var game = new Window())
                game.Run(60.0);
        }
    }
}
