using Krasnobaev.DynamicArrayUtilities.Interfaces;
using System.Collections;

namespace Krasnobaev.DynamicArrayUtilities
{
    /// <summary>
    /// Динамический массив.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива.</typeparam>
    public class DynamicArray<T> : IEnumerable<T>
	{
		private bool _isModified;
		private readonly IRepository<T>? _repository;
		private T[] _items = null!;
		private int _length;
		private int _capacity;

		#region Constants

		/// <summary>
		/// Длина массива по умолчанию.
		/// </summary>
		private const int DefaultCapacity = 8;

		/// <summary>
		/// Коэффициент увеличения массива.
		/// </summary>
		private const int GrowthFactor = 2;

		/// <summary>
		/// Сообщение о некорректной переданной длине массива.
		/// </summary>
		private const string InvalidArrayCapacity = "Длина массива должна быть больше 0.";

		/// <summary>
		/// Сообщение о некорректной переданной коллекции.
		/// </summary>
		private const string InvalidCollectionMessage = "Коллекция не может быть null.";

		/// <summary>
		/// Сообщение о некорректно переданном индексе.
		/// </summary>
		private const string InvalidIndexMessage = "Неверный индекс.";

		/// <summary>
		/// Индекс не удаленного элемента.
		/// </summary>
		private const int UndeletedElementIndex = -1;

		#endregion

		#region Properties

		/// <summary>
		/// Длина массива.
		/// </summary>
		public int Length => _length;

		/// <summary>
		/// Фактическая длина массива.
		/// </summary>
		public int Capacity => _capacity;

		#endregion

		#region Constructors

		/// <summary>
		/// Создает массив с емкостью по умолчанию.
		/// </summary>
		public DynamicArray()
			: this(DefaultCapacity) 
		{
			_isModified = false;
		}

		/// <summary>
		/// Создает массив указанной длины.
		/// </summary>
		/// <param name="capacity">Фактическая длина массива.</param>
		/// <exception cref="ArgumentException">Выбрасывается, если фактическая длина меньше 1.</exception>
		public DynamicArray(int capacity)
		{
			if (capacity <= 0)
			{
				throw new ArgumentException(InvalidArrayCapacity);
			}

			_items = new T[capacity];
			_capacity = capacity;
			_length = 0;
			_isModified = false;
		}


		/// <summary>
		/// Создает динамический массив из <paramref name="collection"/>.
		/// </summary>
		/// <param name="collection">Коллекция элементов.</param>
		public DynamicArray(IEnumerable<T> collection)
		{
			ArgumentNullException.ThrowIfNull(collection);
			AddRange(collection);
			_isModified = false;
		}

		/// <summary>
		/// Создает динамический массив и инициализирует его данными из репозитория.
		/// </summary>
		/// <param name="repository">Репозиторий, из которого загружаются элементы.</param>
		/// <exception cref="ArgumentNullException">Выбрасывается, если переданный репозиторий равен <c>null</c>.</exception>
		public DynamicArray(IRepository<T> repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			var collection = _repository.GetAll().ToArray();
			_items = new T[collection.Length];
			collection.CopyTo(_items, 0);
			_length = collection.Length;
			_capacity = _length;
			_isModified = false;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Сохраняет текущие элементы массива, используя репозиторий.
		/// </summary>
		public void Save()
		{
			if (_isModified && _repository != null)
			{
				_repository.Save(_items.Take(_length));
				_isModified = false;
			}
		}

		/// <summary>
		/// Добавление элемента в конец массива.
		/// </summary>
		/// <param name="item">Добавляемый элемент.</param>
		public void Add(T item)
		{
			EnsureCapacity(_length + 1);
			_items[_length++] = item;
			_isModified = true;
		}

		/// <summary>
		/// Добавляет коллекцию элементов в конец массива.
		/// </summary>
		/// <param name="collection">Коллекция элементов.</param>
		public void AddRange(IEnumerable<T> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection), InvalidCollectionMessage);
			}

			var tempList = new List<T>(collection);
			var count = tempList.Count;

			EnsureCapacity(_length + count);
			tempList.CopyTo(_items, _length);
			_length += count;
			_isModified = true;
		}

		/// <summary>
		/// Удаляет первый найденный элемент.
		/// </summary>
		/// <param name="item">Удаляемый элемент.</param>
		/// <returns>True, если удаление прошло успешно.</returns>
		public bool Remove(T item)
		{
			var index = Array.IndexOf(_items, item, 0, _length);

			if (index == UndeletedElementIndex)
			{
				return false;
			}

			Array.Copy(_items, index + 1, _items, index, _length - index - 1);
			_items[--_length] = default!;
			_isModified = true;

			return true;
		}


		/// <summary>
		/// Вставляет элемент в указанную позицию.
		/// </summary>
		/// <param name="index">Индекс вставки.</param>
		/// <param name="item">Добавляемый элемент.</param>
		/// <returns>True, если вставка прошла успешно.</returns>
		public bool Insert(int index, T item)
		{
			ValidateIndex(index, allowEnd: true);

			EnsureCapacity(_length + 1);
			Array.Copy(_items, index, _items, index + 1, _length - index);
			_items[index] = item;
			_length++;
			_isModified = true;

			return true;
		}

		/// <summary>
		/// Индексатор.
		/// </summary>
		/// <param name="index">Индекс элемента.</param>
		/// <returns>Элемент массива.</returns>
		public T this[int index]
		{
			get
			{
				ValidateIndex(index);

				return _items[index];
			}
			set
			{
				ValidateIndex(index);
				_items[index] = value;
				_isModified = true;
			}
		}

		/// <summary>
		/// Проверка индекса.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="allowEnd"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private void ValidateIndex(int index, bool allowEnd = false)
		{
			var maxIndex = allowEnd 
				? _length 
				: _length--;

			if (index < 0 || index > maxIndex)
			{
				throw new ArgumentOutOfRangeException(nameof(index), InvalidIndexMessage);
			}
		}

		/// <summary>
		/// Увеличивает ёмкость массива.
		/// </summary>
		/// <param name="minCapacity">Минимально необходимая ёмкость.</param>
		private void EnsureCapacity(int minCapacity)
		{
			if (minCapacity <= _capacity)
			{
				return;
			}

			var newCapacity = _capacity * GrowthFactor;

			if (newCapacity < minCapacity)
			{
				newCapacity = minCapacity;
			}

			Array.Resize(ref _items, newCapacity);
			_capacity = newCapacity;
		}

		/// <summary>
		/// Перечислитель элементов массива.
		/// </summary>
		/// <returns>Перечислитель.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			for (var i = 0; i < _length; i++)
			{
				yield return _items[i];
			}
		}

		/// <summary>
		/// Перечислитель, выполняющий итерацию в динамическом массиве.
		/// </summary>
		/// <returns>Объект <see cref="IEnumerator"/>, который можно использовать для перебора элементов.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}