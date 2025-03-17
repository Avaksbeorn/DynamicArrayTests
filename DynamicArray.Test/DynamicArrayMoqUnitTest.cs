using Krasnobaev.DynamicArrayUtilities.Interfaces;
using Krasnobaev.DynamicArrayUtilities;
using Moq;

namespace DynamicArray.Test
{
	/// <summary>
	/// Тесты динамического массива с Moq.
	/// </summary>
	[TestClass]
	public class DynamicArrayMoqUnitTest
	{
		#region Moq
		/// <summary>
		/// Мок репозитория для тестирования взаимодействия с внешними зависимостями.
		/// </summary>
		private Mock<IRepository<int>> _mockRepository = null!;

		/// <summary>
		/// Инициализация тестов: создание мока репозитория.
		/// </summary>
		[TestInitialize]
		public void Setup()
		{
			_mockRepository = new Mock<IRepository<int>>();
		}

		/// <summary>
		/// Тест проверяет, что конструктор класса загружает данные из репозитория.
		/// </summary>
		/// <param name="testData">Тестовые данные, которые возвращает мок репозитория.</param>
		[TestMethod]
		[DataRow(new int[] { 1, 2, 3 })]
		[DataRow(new int[] { 10, 20, 30, 40 })]
		[DataRow(new int[] { })]
		public void Constructor_Repository_LoadsData(int[] testData)
		{
			// Arrange
			_mockRepository.Setup(repo => repo.GetAll()).Returns(testData);

			// Act
			var array = new DynamicArray<int>(_mockRepository.Object);

			// Assert
			CollectionAssert.AreEqual(testData, array.ToArray());
		}

		/// <summary>
		/// Тест проверяет, что метод Save вызывает сохранение данных в репозитории.
		/// </summary>
		[TestMethod]
		public void Save_CallsRepositorySave()
		{
			// Arrange
			var array = new DynamicArray<int>(_mockRepository.Object);
			array.Add(10);

			// Act
			array.Save();

			// Assert
			_mockRepository.Verify(repo => repo.Save(It.IsAny<IEnumerable<int>>()), Times.Once);
		}

		/// <summary>
		/// Тест проверяет, что конструктор возвращает пустой массив, если репозиторий вернул пустую коллекцию.
		/// </summary>
		[TestMethod]
		public void Constructor_EmptyRepository_ReturnsEmptyArray()
		{
			// Arrange
			var emptyData = new List<int>();
			_mockRepository.Setup(repo => repo.GetAll()).Returns(emptyData);

			// Act
			var array = new DynamicArray<int>(_mockRepository.Object);

			// Assert
			Assert.AreEqual(0, array.Length);
			Assert.AreEqual(0, array.Capacity);
			CollectionAssert.AreEqual(emptyData, array.ToArray());
		}

		/// <summary>
		/// Тест проверяет, что метод Save не вызывает сохранение данных в репозитории, если изменения отсутствуют.
		/// </summary>
		/// <param name="testData">Тестовые данные, которые возвращает мок репозитория.</param>
		[TestMethod]
		[DataRow(new int[] { 1, 2, 3 })]
		[DataRow(new int[] { 10, 20, 30 })]
		[DataRow(new int[] { })]
		public void Save_NoChanges_DoesNotCallRepositorySave(int[] testData)
		{
			// Arrange
			_mockRepository.Setup(repo => repo.GetAll()).Returns(testData);

			var array = new DynamicArray<int>(_mockRepository.Object);

			// Act
			array.Save();

			// Assert
			_mockRepository.Verify(repo => repo.Save(It.IsAny<IEnumerable<int>>()), Times.Never);
		}

		/// <summary>
		/// Тест проверяет, что метод Save вызывает сохранение обновленных данных после добавления элемента.
		/// </summary>
		/// <param name="initialData">Начальные данные, загруженные из репозитория.</param>
		/// <param name="elementToAdd">Элемент, который добавляется в массив.</param>
		/// <param name="expectedData">Ожидаемые данные после добавления элемента.</param>
		[TestMethod]
		[DataRow(new int[] { 1, 2, 3 }, 4, new int[] { 1, 2, 3, 4 })]
		[DataRow(new int[] { 10, 20 }, 30, new int[] { 10, 20, 30 })]
		[DataRow(new int[] { }, 5, new int[] { 5 })]
		public void Save_AfterAddingElement_CallsRepositoryWithUpdatedData(int[] initialData, int elementToAdd, int[] expectedData)
		{
			// Arrange
			_mockRepository.Setup(repo => repo.GetAll()).Returns(initialData);

			var array = new DynamicArray<int>(_mockRepository.Object);
			array.Add(elementToAdd);

			// Act
			array.Save();

			// Assert
			_mockRepository.Verify(repo => repo.Save(It.Is<IEnumerable<int>>(data => data.SequenceEqual(expectedData))), Times.Once);
		}

		/// <summary>
		/// Тест проверяет, что метод Save вызывает сохранение обновленных данных после удаления элемента.
		/// </summary>
		/// <param name="initialData">Начальные данные, загруженные из репозитория.</param>
		/// <param name="elementToRemove">Элемент, который удаляется из массива.</param>
		/// <param name="expectedData">Ожидаемые данные после удаления элемента.</param>
		[TestMethod]
		[DataRow(new int[] { 1, 2, 3 }, 2, new int[] { 1, 3 })]
		[DataRow(new int[] { 10, 20, 30 }, 10, new int[] { 20, 30 })]
		[DataRow(new int[] { 5 }, 5, new int[] { })]
		public void Save_AfterRemovingElement_CallsRepositoryWithUpdatedData(int[] initialData, int elementToRemove, int[] expectedData)
		{
			// Arrange
			_mockRepository.Setup(repo => repo.GetAll()).Returns(initialData);

			var array = new DynamicArray<int>(_mockRepository.Object);
			array.Remove(elementToRemove);

			// Act
			array.Save();

			// Assert
			_mockRepository.Verify(repo => repo.Save(It.Is<IEnumerable<int>>(data => data.SequenceEqual(expectedData))), Times.Once);
		}

		/// <summary>
		/// Тест проверяет, что метод Save вызывает сохранение пустых данных после удаления всех элементов.
		/// </summary>
		/// <param name="initialData">Начальные данные, загруженные из репозитория.</param>
		[TestMethod]
		[DataRow(new int[] { 1, 2, 3 })]
		[DataRow(new int[] { 10, 20 })]
		[DataRow(new int[] { 5 })]
		public void Save_AfterClearingArray_CallsRepositoryWithEmptyData(int[] initialData)
		{
			// Arrange
			_mockRepository.Setup(repo => repo.GetAll()).Returns(initialData);

			var array = new DynamicArray<int>(_mockRepository.Object);
			foreach (var item in initialData)
			{
				array.Remove(item);
			}

			// Act
			array.Save();

			// Assert
			_mockRepository.Verify(repo => repo.Save(It.Is<IEnumerable<int>>(data => !data.Any())), Times.Once);
		}

		/// <summary>
		/// Тест проверяет, что конструктор выбрасывает исключение при ошибке в репозитории.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Constructor_ThrowsException_WhenRepositoryFails()
		{
			// Arrange
			_mockRepository.Setup(repo => repo.GetAll()).Throws(new InvalidOperationException("Ошибка при загрузке данных"));

			// Act
			var array = new DynamicArray<int>(_mockRepository.Object);
		}
		#endregion
	}
}
