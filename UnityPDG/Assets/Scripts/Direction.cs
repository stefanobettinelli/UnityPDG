using UnityEngine;

public enum Direction {
	North,
	East,
	South,
	West
}

public static class Directions{

	public const int count = 4;

	private static IntVector2[] directionVectors = {
		new IntVector2(0,1),
		new IntVector2(1,0),
		new IntVector2(0,-1),
		new IntVector2(-1,0)
	};

	/* extention method: vuol dire che posso usarlo come fosse un motedo non statico su un oggetto
	l'oggetto su cui applicare il metodo è del tipo del primo argomento, infatti bisogna aggiungere
	"this" affinche sia possibile usare il metodo come someDirection.ToIntVector2() invece che
	MazeDirections.ToIntVector2(someDirection) */
	public static IntVector2 ToIntVector2 (this Direction direction){
		//uso l'enum come indice del directionVectors per prendere una delle direzioni polari sottoforma di IntVector2
		return directionVectors[(int)direction];
	}

	public static Direction RandomValue{
		get{
			return (Direction)Random.Range(0,count);
		}
	}
}
