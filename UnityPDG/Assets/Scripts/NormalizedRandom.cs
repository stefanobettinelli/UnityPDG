using UnityEngine;
using System.Collections;

public static class NormalizedRandom {
    //generatore di numeri random con distribuzione gaussiana, voglio che le stanze non sia dei rettangoli troppo stretti...
    public static int nextRandomInt(int minValue, int maxValue)
    {
        float mean = (minValue + maxValue) / 2;
        float sigma = (maxValue - mean) / 3;
        Debug.Log("intorno minvalue: " + (mean - sigma) + " maxvalue: " + (mean + sigma));
        return (int)Random.Range(mean - 2 * sigma, mean + 2 * sigma);
    }
}
