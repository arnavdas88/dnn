// -----------------------------------------------------------------------
// <copyright file="DisposableObject.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    /// <summary>
    /// Provides the recommended implementation of <see cref="IDisposable"/> interface. This is an abstract class.
    /// </summary>
    public abstract class DisposableObject : IDisposable
    {
        /// <summary>
        /// Indicates whether the object was already disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Finalizes an instance of the <see cref="DisposableObject"/> class.
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This member overrides <see cref="object.Finalize"/>, and more complete documentation might be available in that topic.
        /// </remarks>
        ~DisposableObject()
        {
            // Use C# destructor syntax for finalization code.
            // This destructor will run only if the Dispose method does not get called.
            // It gives your base class the opportunity to finalize.
            // Do not provide destructors in types derived from this class.

            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of readability and maintainability.
            if (!this.disposed)
            {
                this.Dispose(false);

                this.disposed = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        /// <value>
        /// <b>true</b> if the object is disposed; otherwise, <b>false</b>.
        /// </value>
        /// <remarks>
        /// It is safe to check this property even after object is disposed.
        /// </remarks>
        public bool IsDisposed => this.disposed;

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method frees all unmanaged resources used by the object.
        /// The method invokes the protected <see cref="Dispose(bool)"/> method with the <c>disposing</c> parameter set to <b>true</b>.
        /// </para>
        /// <para>
        /// Call <see cref="Dispose()"/> when you are finished using the object.
        /// The <see cref="Dispose()"/> method leaves the object in an unusable state.
        /// After calling <see cref="Dispose()"/>, you must release all references to the object so the garbage collector can reclaim the memory that the object was occupying.
        /// </para>
        /// </remarks>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Dispose(true);

                this.disposed = true;
            }

            // Take yourself off the Finalization queue to prevent finalization code for this object from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Infrastructure. Releases the unmanaged resources used by the object and, optionally, releases managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        /// <remarks>
        /// <para>
        /// This method is called by the public <see cref="Dispose()"/> method and the <see cref="Finalize"/> method.
        /// <see cref="Dispose()"/> invokes the protected <see cref="Dispose(bool)"/> method with the <paramref name="disposing"/> parameter set to <b>true</b>.
        /// <see cref="Finalize"/> invokes <see cref="Dispose(bool)"/> with <paramref name="disposing"/> set to <b>false</b>.
        /// </para>
        /// <para>
        /// When the <paramref name="disposing"/> parameter is <b>true</b>,
        /// this method releases all resources held by any managed objects that this object references.
        /// This method invokes the <see cref="Dispose()"/> method of each referenced object.
        /// </para>
        /// <para>
        /// <see cref="Dispose(bool)"/> executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly or indirectly by a user's code.
        /// Managed and unmanaged resources can be disposed.
        /// If disposing equals false, the method has been called by the runtime from inside the finalizer and you should not reference other objects.
        /// Only unmanaged resources can be disposed.
        /// </para>
        /// </remarks>
        protected abstract void Dispose(bool disposing);
    }
}
