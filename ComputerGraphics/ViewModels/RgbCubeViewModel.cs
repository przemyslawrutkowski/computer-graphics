using ComputerGraphics.MVVM;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace ComputerGraphics.ViewModels
{
    public class RgbCubeViewModel : ViewModelBase
    {
        private readonly Transform3DGroup _transformGroup;
        private readonly QuaternionRotation3D _rotation;
        private readonly RotateTransform3D _rotateTransform;

        public ImageBrush FrontFaceTexture { get; }
        public ImageBrush BackFaceTexture { get; }
        public ImageBrush LeftFaceTexture { get; }
        public ImageBrush RightFaceTexture { get; }
        public ImageBrush TopFaceTexture { get; }
        public ImageBrush BottomFaceTexture { get; }

        public Transform3DGroup CubeTransform => _transformGroup;

        private double _rotationX;
        public double RotationX
        {
            get => _rotationX;
            set
            {
                _rotationX = value;
                UpdateRotation();
                RaisePropertyChanged();
            }
        }

        private double _rotationY;
        public double RotationY
        {
            get => _rotationY;
            set
            {
                _rotationY = value;
                UpdateRotation();
                RaisePropertyChanged();
            }
        }

        private double _rotationZ;
        public double RotationZ
        {
            get => _rotationZ;
            set
            {
                _rotationZ = value;
                UpdateRotation();
                RaisePropertyChanged();
            }
        }

        public RgbCubeViewModel()
        {
            _rotation = new QuaternionRotation3D(new Quaternion(0, 0, 0, 1));
            _rotateTransform = new RotateTransform3D(_rotation);
            _transformGroup = new Transform3DGroup();
            _transformGroup.Children.Add(_rotateTransform);

            FrontFaceTexture = GenerateFaceTexture(Colors.Red, Colors.Yellow, Colors.Magenta, Colors.White);
            BackFaceTexture = GenerateFaceTexture(Colors.Green, Colors.Black, Colors.Cyan, Colors.Blue);
            LeftFaceTexture = GenerateFaceTexture(Colors.Black, Colors.Red, Colors.Blue, Colors.Magenta);
            RightFaceTexture = GenerateFaceTexture(Colors.Yellow, Colors.Green, Colors.White, Colors.Cyan);
            TopFaceTexture = GenerateFaceTexture(Colors.Black, Colors.Green, Colors.Red, Colors.Yellow);
            BottomFaceTexture = GenerateFaceTexture(Colors.Magenta, Colors.White, Colors.Blue, Colors.Cyan);
        }

        private void UpdateRotation()
        {
            Quaternion rotationX = new Quaternion(new Vector3D(1, 0, 0), RotationX);
            Quaternion rotationY = new Quaternion(new Vector3D(0, 1, 0), RotationY);
            Quaternion rotationZ = new Quaternion(new Vector3D(0, 0, 1), RotationZ);

            Quaternion combinedRotation = rotationX * rotationY * rotationZ;
            _rotation.Quaternion = combinedRotation;
        }

        private ImageBrush GenerateFaceTexture(Color topLeft, Color topRight, Color bottomLeft, Color bottomRight)
        {
            int size = 256;
            WriteableBitmap bitmap = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgr32, null);
            int[] pixels = new int[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double u = x / (double)(size - 1);
                    double v = y / (double)(size - 1);

                    byte r = (byte)((topLeft.R * (1 - u) * (1 - v)) +
                                    (topRight.R * u * (1 - v)) +
                                    (bottomLeft.R * (1 - u) * v) +
                                    (bottomRight.R * u * v));

                    byte g = (byte)((topLeft.G * (1 - u) * (1 - v)) +
                                    (topRight.G * u * (1 - v)) +
                                    (bottomLeft.G * (1 - u) * v) +
                                    (bottomRight.G * u * v));

                    byte b = (byte)((topLeft.B * (1 - u) * (1 - v)) +
                                    (topRight.B * u * (1 - v)) +
                                    (bottomLeft.B * (1 - u) * v) +
                                    (bottomRight.B * u * v));

                    int color = (255 << 24) | (r << 16) | (g << 8) | b;
                    pixels[y * size + x] = color;
                }
            }

            bitmap.WritePixels(new Int32Rect(0, 0, size, size), pixels, size * 4, 0);
            return new ImageBrush(bitmap);
        }
    }
}
