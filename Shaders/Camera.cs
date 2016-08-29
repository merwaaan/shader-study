using OpenTK;
using System;
using OpenTK.Input;

namespace Shaders
{
    public class Camera : ITransform, IEye
    {
        public Matrix4 Transform { get; set; }

        public Matrix4 ViewMatrix => _transform;
        public Matrix4 ProjectionMatrix { get; }

        private Matrix4 _transform;

        private float _cameraRotation;
        private float _cameraAngularSpeed = 0.005f;
        private float _cameraDistance = 1;
        private float _cameraZoomSpeed = 0.02f;

        private readonly App _app;

        public Camera(App app)
        {
            _app = app;

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, _app.Width / (float)_app.Height, 0.005f, 100f);
            UpdateViewMatrix();
        }
        internal void OnMouseMove(MouseMoveEventArgs ev)
        {
            _cameraRotation += _cameraAngularSpeed * ev.XDelta;
            UpdateViewMatrix();
        }

        internal void OnMouseWheel(MouseWheelEventArgs ev)
        {
            _cameraDistance += _cameraZoomSpeed * -ev.DeltaPrecise;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            var eye = new Vector3(
                _cameraDistance * (float)Math.Cos(_cameraRotation),
                0,
                _cameraDistance * (float)Math.Sin(_cameraRotation));

            _transform = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);
        }
    }
}
