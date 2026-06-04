namespace Monster
{
    public static class EntitySerialId
    {
        private static int mNext = 0;

        public static int Next() => ++mNext;
    }
}
