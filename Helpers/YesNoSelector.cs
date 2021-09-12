using System;

namespace YWB.AntidetectAccountParser.Helpers
{
    public static class YesNoSelector
    {
        public static bool ReadAnswerEqualsYes(string message=null)
        {
            if (!string.IsNullOrEmpty(message)) Console.Write($"{message}(Y/N)");
            var answer = Console.ReadKey();
            Console.WriteLine();
            return answer.KeyChar == 'Y' || answer.KeyChar == 'y'|| answer.KeyChar=='н';
        }

        public static int GetMenuAnswer(int maxCount)
        {
            bool answerIsOk = false;
            int index=0;
            while (!answerIsOk)
            {
                Console.Write("Your choice:");
                var answer = Console.ReadLine();
                if (!int.TryParse(answer, out _)) continue;
                index = int.Parse(answer);
                if (index < 0 || index > maxCount) continue;
                answerIsOk=true;
            }
            return index;
        }
    }
}
