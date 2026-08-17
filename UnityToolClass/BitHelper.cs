using System.Collections.Generic;

namespace Common
{
    /// <summary>
    /// 位运算助手
    /// </summary>
    public static class BitHelper
    {
        /// <summary>
        /// 获得2的n次幂中的幂的次数
        /// </summary>
        /// <param name="value">2的n次幂</param>
        /// <returns></returns>
        public static int GetPower(int value)
        {
            if (value <= 0 || (value & (value - 1)) != 0)
            {
                //非2的n次幂
                return -1;
            }

            int n = 0;
            while (value > 0)
            {
                value >>= 1;
                n++;
            }

            return n - 1;
        }

        /// <summary>
        /// 获取位数对应的值为1的位数序号
        /// 例：1, 3 则证明第1位和第3位的值为1
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int[] GetPowers(int value)
        {
            List<int> ones = new List<int>();
            int bit = -1;
            while (value > 0)
            {
                bit++;
                if ((value & 1) == 1)
                {
                    ones.Add(bit);
                }
                value >>= 1;
            }
            return ones.ToArray();
        }


    }
}

