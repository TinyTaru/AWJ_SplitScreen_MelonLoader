using System;

namespace UnityEngine.Purchasing.Security;

public class GooglePlayTangle
{
	private static byte[] data = Convert.FromBase64String("39ZPkMAk3+aWBJswulZEaSFk9tzUlMmSTxa7aQuq+OFTY9t5UQ5BoATRUexh2BdGw2cyr9uT46ZwoA4pR8TKxfVHxM/HR8TExWTedhE8WKuG6khkrFFSpn5hcAsh0aO2arZhvfVHxOf1yMPM70ONQzLIxMTEwMXGAfpswk94kJOMhNxBShXGqMQtHBH8t+2dAHUhl+b7idG2q375hqkG1xw5/7etiIoRQU6CItj3XHd+pjZRF9zz1UjrTgtOkQ3X36lNX/vfrRTlLdi9nKLEDp30B8ob+xjRnd06BdekT6OZtG0X7gaIW8hx3NWn6+q2hF0+MoAEJfKZN1JJSGC/sZCWPxzmNzUOAEnVGf5Jl4XnJNlzpqKl0DqDsUFlVqgILsfGxMXE");

	private static int[] order = new int[15]
	{
		3, 5, 4, 5, 8, 5, 7, 7, 13, 13,
		11, 11, 13, 13, 14
	};

	private static int key = 197;

	public static readonly bool IsPopulated = true;

	public static byte[] Data()
	{
		if (!IsPopulated)
		{
			return null;
		}
		return Obfuscator.DeObfuscate(data, order, key);
	}
}
