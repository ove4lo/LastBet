using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] // с помощью этого можно задавать в Unity зн-ие
    private int boardHeight, boardWidth; // высота и ширина игрового поля
    [SerializeField]
    private GameObject[] gamePieces; // наши слоты

    private GameObject _board; // наша доска Game Board
    private GameObject[,] _gameBoard; // массив созданных слотов
    private Vector3 _offset = new Vector3(0, 0, -1);

    // Метод старта
    void Start()
    {
        // Находим объект "GameBoard" в сцене — это родитель всех слотов
        _board = GameObject.Find("GameBoard");

        // Создаём 2D-массив для хранения ссылок на все иконки (3 строки × 5 столбцов)
        _gameBoard = new GameObject[boardHeight, boardWidth];

        // Перебираем все строки (i) и столбцы (j)
        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                // Находим слот с именем типа "0 0", "0 1", "1 2" и т.д.
                GameObject gridPosition = _board.transform.Find(i + " " + j).gameObject;

                // Выбираем случайный префаб иконки из массива gamePieces
                GameObject pieceType = gamePieces[Random.Range(0, gamePieces.Length)];

                // Создаём иконку чуть впереди слота (по Z = -1), чтобы была поверх
                GameObject thisPiece = Instantiate(pieceType, gridPosition.transform.position + _offset, Quaternion.identity);

                // Даём иконке понятное имя 
                thisPiece.name = pieceType.name;

                // Делаем слот родителем иконки — порядок в иерархии будет чистым
                thisPiece.transform.parent = gridPosition.transform;

                // Сохраняем ссылку на иконку в массив — потом будем проверять матчи именно по нему
                _gameBoard[i, j] = thisPiece;
            }
        }
    }

    // Метод спина
    public void Spin()
    {
        // Перебираем все строки (i) и столбцы (j) — заново заполняем ВСЮ доску
        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                // Находим слот по имени "0 0", "0 1", "1 2" и т.д.
                GameObject gridPosition = _board.transform.Find(i + " " + j).gameObject;

                // Уничтожаем старую иконку, если она есть (childCount > 0)
                if (gridPosition.transform.childCount > 0)
                {
                    GameObject destroyPiece = gridPosition.transform.GetChild(0).gameObject;
                    Destroy(destroyPiece);  // Освобождаем память и слот
                }

                // Выбираем случайный префаб новой иконки
                GameObject pieceType = gamePieces[Random.Range(0, gamePieces.Length)];

                // Создаём новую иконку поверх слота (Z = -1)
                GameObject thisPiece = Instantiate(pieceType,
                    gridPosition.transform.position + _offset,
                    Quaternion.identity);

                // Даём имя 
                thisPiece.name = pieceType.name;

                // Делаем слот родителем — чистая иерархия
                thisPiece.transform.parent = gridPosition.transform;

                // Обновляем массив _gameBoard — ссылки на НОВЫЕ иконки
                _gameBoard[i, j] = thisPiece;
            }
        }
    }

}
