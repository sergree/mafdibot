using System;

namespace MafDiBot
{
    static class GameRules
    {
        // Минимальная задержка
        public const int minInterval = 3000;
        // Интервал на запись
        public const int intervalRegistration = 40000; //
        // Интервал на запись (дополнительное время)
        public const int intervalRegistrationAdditional = 20000; //
        // Интервал на подготовку к игре
        public const int intervalCasting = 20000; //
        // Интервал ночи
        public const int intervalNight = 40000; // 70000
        // Интервал вывода сообщений днём
        public const int intervalDay = 8000; //
        // Интервал первого голосования
        public const int intervalVotingFirst = 40000; //
        // Интервал первого последнего слова
        public const int intervalLastWordFirst = 30000; //
        // Интервал второго голосования
        public const int intervalVotingSecond = 40000; //
        // Интервал второго последнего слова
        public const int intervalLastWordSecond = 20000; //
        // Остаточный интервал, если все проголосовали
        public const int intervalVotingAdditional = 10000;
        // Интервал паузы перед следующей регистрацией
        public const int intervalPause = 3000;

        // Минимальное количество игроков
        public const int minNumPlayers = 3;
        // Минимальное количество игроков для отключения демо-режима
        //public const int minNumNoDemoPlayers = 6;
        // Максимальное количество игроков
        public const int maxNumPlayers = 20; //В полной версии = 100
        
        // Максимальное количество мафиози
        public const int maxNumMafia = 10;

        // Константы расчета количества мафиози: вычитаемое
        public const double mafiaSub = 1.0; //В полной версии = 3.0
        // Константы расчета количества мафиози: делитель
        public const double mafiaDiv = 4.0;
        // Расчет количества мафиози
        public static int GetMafiaCount(int players)
        {
            int numMafia = Convert.ToInt32(Math.Round((players - mafiaSub) / mafiaDiv));
            if (numMafia < 1)
            {
                return 1;
            }
            if (numMafia > maxNumMafia)
            {
                return maxNumMafia;
            }
            return numMafia;
        }

        // Мин. количество игроков для присутствия маньяка
        public const int inGameManiac = 4;
        // Мин. количество игроков для присутствия доктора
        public const int inGameDoctor = 5;

        // Сколько раз может хилить доктор одного игрока, если превышено = убийство
        public const int maxHealNum = 2;

        // Количество ходов, которые можно пропустить до смерти
        const int skip1Spec = 6;
        const int skip2Spec = 29;
        const int skip3Spec = 41;
        const int skip4Spec = 53;
        public static int GetNumSkip(int PlayersCount)
        {
            if (PlayersCount < skip1Spec) return 0;
            if (PlayersCount < skip2Spec) return 1;
            if (PlayersCount < skip3Spec) return 2;
            if (PlayersCount < skip4Spec) return 3;
            return 4;
        }

        // Коэффициент Линча
        public const int coefLynch = 2;
    }
}
