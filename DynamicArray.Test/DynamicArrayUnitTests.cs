using Norbit.Crm.Krasnobaev.DynamicArrayUtilities;
using System.Linq.Expressions;

namespace Norbit.Crm.krasnobaev.DynamicArray.Test
{
    /// <summary>
    /// ����� ������������� �������.
    /// </summary>
    [TestClass]
    public class DynamicArrayUnitTests
    {
        #region Indexer Tests
        /// <summary>
        /// �������� ����������� ����������� ��������.
        /// </summary>
        [TestMethod]
        [DataRow(0, 1)]
        [DataRow(1, 2)]
        [DataRow(2, 3)]
        public void Indexer_ValidIndex_ReturnsCorrectValue(int validIndex, int arrayValue)
        {
            var array = new DynamicArray<int> { 1, 2, 3 };

            Assert.AreEqual(arrayValue, array[validIndex]);
        }

        /// <summary>
        /// �������� ����������� ������������� ��������.
        /// </summary>
        /// <param name="invalidIndex"></param>
        [TestMethod]
        [DataRow(-1)]
        [DataRow(7)]
        [DataRow(4)]
        [DataRow(800)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Indexer_InvalidIndex_ThrowsException(int invalidIndex)
        {
            var array = new DynamicArray<int> { 1, 2, 3 };
            _ = array[invalidIndex];
        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// ����������� �� ���������.
        /// </summary>
        [TestMethod]
        public void Constructor_DefaultCapacity_IsCorrect()
        {
            var array = new DynamicArray<int>();

            Assert.AreEqual(0, array.Length);
            Assert.AreEqual(8, array.Capacity);
        }

        /// <summary>
        /// �������� ������������ ����������� ��������.
        /// </summary>
        /// <param name="capacity"></param>
        [TestMethod]
        [DataRow(5)]
        [DataRow(100)]
        public void Constructor_ValidCapacity_Success(int capacity)
        {
            var array = new DynamicArray<int>(capacity);

            Assert.AreEqual(0, array.Length);
            Assert.AreEqual(capacity, array.Capacity);
        }

        /// <summary>
        /// ����������� � ��������� ���������.
        /// </summary>
        /// <param name="collection">����������� � ������ ���������.</param>
        [TestMethod]
        [DataRow(new int[] { 1, 4, 6, -33, 44, 1000 })]
        [DataRow(new int[] { 100002, 3233, 123, 334, 404, 602 })]
        [DataRow(new int[] { -10, -100, -1000, -10000, -1234560 })]
        public void Constructor_ValidCollection_Success(int[] collection)
        {
            var array = new DynamicArray<int>(collection);

            Assert.AreEqual(array.Length, collection.Length);
            Assert.AreEqual(array.Capacity, collection.Length);
            Assert.IsTrue(array.SequenceEqual(collection));
        }

        /// <summary>
        /// �������� ������������ ������������� ��������.
        /// </summary>
        /// <param name="invalidCapacity"></param>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-5)]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_InvalidCapacity_ThrowsException(int invalidCapacity)
        {
            var array = new DynamicArray<int>(invalidCapacity);
        }

        /// <summary>
        /// �������� ������ ���������.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullCollection_ThrowsException()
        {
            IEnumerable<int> collection = null;
            var array = new DynamicArray<int>(collection);
        }

        #endregion

        #region Add and AddRange Tests

        /// <summary>
        /// ���������� ��������.
        /// </summary>
        /// <param name="element">������������ �������.</param>
        [TestMethod]
        [DataRow(10)]
        [DataRow(-100)]
        [DataRow(0)]
        public void Add_ValidElement_Success(int element)
        {
            var array = new DynamicArray<int>();
            array.Add(element);

            Assert.AreEqual(1, array.Length);
            Assert.AreEqual(8, array.Capacity);
            Assert.IsTrue(array[0] == element);
        }

        /// <summary>
        /// ���������� �������� ��������� � ����� �������.
        /// </summary>
        [TestMethod()]
        [DataRow(new int[] { 0, 16, 20, 1034123, 1312313123 })]
        [DataRow(new int[] { -10, -100, -1000 })]
        [DataRow(new int[] { 1, 20, 32322, -454513, 0 })]
        public void AddRange_ValidCollection_Success(int[] collection)
        {
            var array = new DynamicArray<int>();

            array.AddRange(collection);

            CollectionAssert.AreEqual(collection, array.ToArray());
        }

        /// <summary>
        /// ���������� ����������� ����������� � ����� �������.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddRange_NullCollection_ThrowsException()
        {
            var array = new DynamicArray<int>();

            array.AddRange(null);
        }

        #endregion

        #region Remove Tests

        /// <summary>
        /// �������� ��������.
        /// </summary>
        /// <param name="collection">����������� ���������.</param>
        /// <param name="element">��������� �������.</param>
        [TestMethod]
        [DataRow(new int[] { 2, 4 }, 4)]
        [DataRow(new int[] { 4, 3, 9 }, 3)]
        [DataRow(new int[] { 2, 3, 9 }, 2)]
        public void Remove_ValidElement_Success(int[] collection, int element)
        {
            var array = new DynamicArray<int>(collection);
            var initialLength = array.Length;
            var initialCapacity = array.Capacity;

            Assert.IsTrue(array.Remove(element));
            Assert.IsFalse(array.Equals(collection));
            Assert.AreEqual(initialLength - 1, array.Length);
            Assert.AreEqual(initialCapacity, array.Capacity);
        }

        /// <summary>
        /// �������� ��������������� ��������.
        /// </summary>
        [TestMethod]
        public void Remove_NonExistingElement_ReturnsFalse()
        {
            var array = new DynamicArray<int> { 1, 2, 3 };

            Assert.IsFalse(array.Remove(10));
        }

        #endregion

        #region Insert Tests

        /// <summary>
        /// ������� �������� �� �������.
        /// </summary>
        /// <param name="index">������ �������.</param>
        /// <param name="element">������� ��� �������.</param>
        [TestMethod]
        [DataRow(2, 100)]
        [DataRow(0, -50)]
        [DataRow(3, 0)]
        public void Insert_ValidIndex_InsertsElement(int index, int element)
        {
            var array = new DynamicArray<int> { 1, 2, 3 };
            array.Insert(index, element);

            Assert.AreEqual(array[index], element);
        }

        /// <summary>
        /// ������� � ������������ ������.
        /// </summary>
        /// <param name="invalidIndex">������ �������.</param>
        [TestMethod]
        [DataRow(-1)]
        [DataRow(4)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Insert_InvalidIndex_ThrowsException(int invalidIndex)
        {
            var array = new DynamicArray<int> { 1, 2, 3 };
            array.Insert(invalidIndex, 10);
        }

        #endregion

        #region Enumerator Tests

        /// <summary>
        /// �������� �������������.
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { 1, 2, 3 })]
        [DataRow(new int[] { 4, 5, 6, 7 })]
        public void Enumerator_IteratesThroughElements(int[] values)
        {
            var array = new DynamicArray<int>(values);
            var list = new List<int>();
            foreach (var item in array)
            {
                list.Add(item);
            }
            CollectionAssert.AreEqual(values.ToList(), list);
        }

        /// <summary>
        /// �������� ������������� � ������ ���������.
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { })]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Enumerator_IteratesNullElements_ThrowsException(int[] values)
        {
            var array = new DynamicArray<int>(values);
            var list = new List<int>();
            foreach (var item in array)
            {
                list.Add(item);
            }
            CollectionAssert.AreEqual(values.ToList(), list);
        }

        #endregion
    }

}