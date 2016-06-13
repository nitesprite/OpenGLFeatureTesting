using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKSandbox
{
    public partial class Form1 : Form
    {
        #region Class Variables

        private float dx = 0.0f, dy = 0.0f, dz = 0f, halfdx = 0f, halfdy = 0f, halfdz = 0f;
        private decimal xMin = 9999999m, xMax = 0m, yMin = 999999m, yMax = 0m, zMin = 999999m, zMax = 0m;
        private float moveX = 0f, moveY = 0f, moveZ = 0f;
        private Vector3 cameraPosition;
        private Vector3 cameraLookAt;
        private Vector3 cameraLookUp;
        private float cameraDistance = 0f;
        private float maxCameraDist = 100f;
        private float nearPlane = 0f;
        private float farPlane = 50.0f;
        private float rotateAngleX = 0.0f, rotateAngleY = 0.0f, rotateAngleZ = 0.0f;
        private float angleOfRotation = 1f;
        private float zScaleFactor = 10;
        private bool drawing = false;

        private Bitmap textureBitmap;

        #region Shader Variables

        // Shader IDs for line object shader
        private Int32 shaderProgramID = -1;
        private Int32 vertexShaderID = -1;
        private Int32 fragmentShaderID = -1;

        // Shader IDs for text shader
        private Int32 textShaderProgramID = -1;
        private Int32 textVertexShaderID = -1;
        private Int32 textFragmentShaderID = -1;

        // Attributes and Uniforms for line object shader
        private Int32 attribute_color;
        private Int32 attribute_position;
        private Int32 uniform_mview;

        // Attributes and Uniforms for text object shader
        private Int32 attribute_texture_position;
        private Int32 attribute_textture_coord;
        private Int32 uniform_modelview;
        private Int32 uniform_texture_array;
        private Int32 uniform_texture_layer;

        #endregion Shader Variables

        #region Graphic Objects

        private UInt32 VBOlines;                    // Line object Vertex Buffer Object
        private UInt32 VBOtext;                     // Text labels Vertex Buffer Object

        private GLPositionColored2[] lineVerts;         // vertex data for lines

        private UInt32 linesIBO;                    // Indices Buffer Object

        private UInt32[] lineIndices;

        private Int32 lineVertexCounter = 0;

        private Int32 textTextureId = 0;
        private GLPositionText[] textPosition;

        #endregion Graphic Objects

        private Rectangle rect = new Rectangle();
        private float xScale, yScale, scale, windowScale;
        private float siteAreaSizeX = 0f;
        private float siteAreaSizeY = 0f;
        private float siteAreaSizeZ = 0f;
        private Int32 screenAreaSizeX = 600;
        private Int32 screenAreaSizeY = 600;
        private static float normalisedSize = 2.0f;
        private static float viewWidth = normalisedSize, viewHeight = normalisedSize, viewDepth = normalisedSize, zoom = 1.0f;
        private Size windowSize = new Size(0, 0);
        private bool windowInitialised = false;

        private Object[] textTextureList = new object[10];      // Array of 10 lists. Each list represents a number (0 - 9) texture and contains positions for each texture

        private Matrix4 modelMatrix;
        private Matrix4 projectionMatrix;
        private Matrix4 lookat;
        private Matrix4 ModelViewProjectionMatrix;

        private List<LineObjects> lineObjects = new List<LineObjects>();

        #endregion Class Variables

        #region Constructor

        public Form1()
        {
            InitializeComponent();

        }

        #endregion Constructor

        #region Initialise

        private bool LoadData()
        {
            FileStream streamIn;

            try
            {
                streamIn = new FileStream("ObjectData.txt", FileMode.Open, FileAccess.Read);
            }
            catch (IOException ex)
            {
                return false;
            }

            TextReader fileIn = new StreamReader(streamIn);

            //int fn = filename.LastIndexOf('\\');

            string lineIn;
            string[] parms = new string[6];

            while (fileIn.Peek() != -1)
            {
                lineIn = fileIn.ReadLine();
                parms = lineIn.Split(',');                                      // Parse input line using comma

                if (parms.Length == 6)
                {
                    vertex start = new vertex(Convert.ToDecimal(parms[0]), Convert.ToDecimal(parms[1]), Convert.ToDecimal(parms[2]));
                    vertex end = new vertex(Convert.ToDecimal(parms[3]), Convert.ToDecimal(parms[4]), Convert.ToDecimal(parms[5]));

                    // Get max and min X values
                    if (Convert.ToDecimal(parms[0]) < xMin)
                    {
                        xMin = Convert.ToDecimal(parms[0]);
                    }
                    if (Convert.ToDecimal(parms[3]) < xMin)
                    {
                        xMin = Convert.ToDecimal(parms[3]);
                    }
                    if (Convert.ToDecimal(parms[0]) > xMax)
                    {
                        xMax = Convert.ToDecimal(parms[0]);
                    }
                    if (Convert.ToDecimal(parms[3]) > xMax)
                    {
                        xMax = Convert.ToDecimal(parms[3]);
                    }

                    // Get max and min Y values
                    if (Convert.ToDecimal(parms[1]) < yMin)
                    {
                        yMin = Convert.ToDecimal(parms[1]);
                    }
                    if (Convert.ToDecimal(parms[4]) < yMin)
                    {
                        yMin = Convert.ToDecimal(parms[4]);
                    }
                    if (Convert.ToDecimal(parms[1]) > yMax)
                    {
                        yMax = Convert.ToDecimal(parms[1]);
                    }
                    if (Convert.ToDecimal(parms[4]) > yMax)
                    {
                        yMax = Convert.ToDecimal(parms[4]);
                    }

                    // Get max and min Z values
                    if (Convert.ToDecimal(parms[2]) < zMin)
                    {
                        zMin = Convert.ToDecimal(parms[2]);
                    }
                    if (Convert.ToDecimal(parms[5]) < zMin)
                    {
                        zMin = Convert.ToDecimal(parms[5]);
                    }
                    if (Convert.ToDecimal(parms[2]) > zMax)
                    {
                        zMax = Convert.ToDecimal(parms[2]);
                    }
                    if (Convert.ToDecimal(parms[5]) > zMax)
                    {
                        zMax = Convert.ToDecimal(parms[5]);
                    }

                    LineObjects lineObject = new LineObjects(start, end);

                    lineObjects.Add(lineObject);
                }
            }

            // In case where all Z values are zero set a default for max Z value = 2
            if (zMax == 0)
            {
                zMax = 2m;
            }

            return true;
        }

        private void OnGL_Load(object sender, EventArgs e)
        {
            // Initialise Shader Program
            InitialiseShaderProgram();

            // Initialise Buffers
            InitialiseBuffers();

            // Initialise Textures
            InitialiseTextures();

            // Load Data
            LoadData();

            xMin -= 1m;
            xMax += 1m;
            yMin -= 1m;
            yMax += 1m;

            // Initialise Graphics
            InitialiseGraphics();

            ShowLineObjects();

            // Set background colour
            GL.ClearColor(Color.Blue);
            CheckForGL_Error("Set Clear Colour");

            // Set Point Size
            GL.PointSize(5);
            CheckForGL_Error("Set Point Size");

            UpdateFrame();
            UpdateTextureFrame();

            glControl1.Invalidate();

            Application.Idle += Application_Idle;
        }

        private void SetupViewport()
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height); // Use all of the controls painting area
            CheckForGL_Error("Viewport");
        }

        private void InitialiseShaderProgram()
        {
            // XSection Numbers shader
            textShaderProgramID = GL.CreateProgram();
            CheckForGL_Error("Create Shader XSectionShaderProgramID");

            // Load vertex and fragment shaders
            LoadShader("TextVertexShader.glsl", ShaderType.VertexShader, textShaderProgramID, out textVertexShaderID);
            LoadShader("TextFragmentShader.glsl", ShaderType.FragmentShader, textShaderProgramID, out textFragmentShaderID);

            // Link shader program
            GL.LinkProgram(textShaderProgramID);
            CheckForGL_Error("Link Program XSectionShaderProgramID");

            string info = "";
            info = GL.GetProgramInfoLog(textShaderProgramID);
            CheckForGL_Error("Get Program XSectionShaderProgramID Info " + info);

            attribute_texture_position = GL.GetAttribLocation(textShaderProgramID, "position3D");
            CheckForGL_Error("Get Attribute Text XSectionShaderProgramID Location");

            attribute_textture_coord = GL.GetAttribLocation(textShaderProgramID, "texturecoordinates");
            CheckForGL_Error("Get Attribute Text Texture XSectionShaderProgramID Location");

            uniform_modelview = GL.GetUniformLocation(textShaderProgramID, "modelview");
            CheckForGL_Error("Get Uniform Location x_modelview Modelview XSectionShaderProgramID");

            uniform_texture_array = GL.GetUniformLocation(textShaderProgramID, "textureArray");
            CheckForGL_Error("Get Uniform Location x_intexlayer XSectionShaderProgramID");

            uniform_texture_layer = GL.GetUniformLocation(textShaderProgramID, "texurelayer");
            CheckForGL_Error("Get Uniform Location x_intexlayer XSectionShaderProgramID");

            Console.Write("attribute_texture_position: {0}, attribute_textture_coord: {1}, uniform_texture_layer: {2}, uniform_modelview: {3}, uniform_texture_array: {4}\n", attribute_texture_position, attribute_textture_coord, uniform_texture_layer, uniform_modelview, uniform_texture_array);

            if (attribute_texture_position == -1 || attribute_textture_coord == -1 || uniform_texture_layer == -1 || uniform_modelview == -1 || uniform_texture_array == -1)
            {
                Console.WriteLine("Error binding text attributes XSectionShaderProgramID");
            }

            shaderProgramID = GL.CreateProgram();
            CheckForGL_Error("Create Shader shaderProgramID");

            // Load vertex and fragment shaders
            LoadShader("VertexShader.glsl", ShaderType.VertexShader, shaderProgramID, out vertexShaderID);
            LoadShader("FragmentShader.glsl", ShaderType.FragmentShader, shaderProgramID, out fragmentShaderID);

            // Link shader program
            GL.LinkProgram(shaderProgramID);
            CheckForGL_Error("Link Program shaderProgramID");

            Console.WriteLine(GL.GetProgramInfoLog(shaderProgramID));
            CheckForGL_Error("Get Program shaderProgramID Info");

            attribute_position = GL.GetAttribLocation(shaderProgramID, "vposition");
            CheckForGL_Error("Get Attribute attribute_position Location shaderProgramID");

            attribute_color = GL.GetAttribLocation(shaderProgramID, "vcolor");
            CheckForGL_Error("Get Attribute attribute_color Location shaderProgramID");

            uniform_mview = GL.GetUniformLocation(shaderProgramID, "vmodelview");
            CheckForGL_Error("Get Uniform Location Modelview shaderProgramID");

            Console.Write("attribute_position: {0}, attribute_color: {1}, uniform_mview: {2}\n", attribute_position, attribute_color, uniform_mview);

            if (attribute_position == -1 || attribute_color == -1 || uniform_mview == -1)
            {
                Console.WriteLine("Error binding attributes shaderProgramID");
            }

        }

        private void LoadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);

            CheckForGL_Error("Create Shader");

            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
                CheckForGL_Error("Read Shader Source");
            }

            GL.CompileShader(address);
            CheckForGL_Error("Compile Shader");

            GL.AttachShader(program, address);
            CheckForGL_Error("Attach Shader");

            Console.WriteLine(GL.GetShaderInfoLog(address));
            CheckForGL_Error("Get Shader Info (Write Console)");
        }

        private void InitialiseBuffers()
        {
            // Object Text Buffer create
            GL.GenBuffers(1, out VBOtext);
            CheckForGL_Error("GenBuffer VBOtext");

            // Object Lines Buffer create
            GL.GenBuffers(1, out VBOlines);
            CheckForGL_Error("GenBuffer VBOlines");

            GL.GenBuffers(1, out linesIBO);
            CheckForGL_Error("GenBuffer linesIBO");
        }

        private void InitialiseTextures()
        {
            CreateIndexedTexture();
        }

        private void InitialiseGraphics()
        {
            dx = (float)(xMax - xMin);
            halfdx = dx / 2;
            dy = (float)(yMax - yMin);
            halfdy = dy / 2;

            zMin = 0;
            dz = (float)((zMax - zMin));
            halfdz = dz / 2;

            //cameraDistance = (float)-zMin;
            cameraDistance = 0.02f;
            nearPlane = 0f;
            maxCameraDist = (float)farPlane;

            // Calculate camera view point
            cameraPosition = new Vector3(0, 0, cameraDistance);
            //cameraLookAt = new Vector3(0, 0, (float)-halfdz);
            cameraLookAt = new Vector3(0, 0, 0);
            cameraLookUp = new Vector3(0.0f, 1.0f, 0.0f);

            moveX = 0; moveY = 0;

            InitialiseWindow();
            SetupViewport();
        }

        public void InitialiseWindow()
        {
            siteAreaSizeX = dx;
            siteAreaSizeY = dy;
            siteAreaSizeZ = dz;

            rect = GetScreen();                             // Get display screen size
            if (rect.Left == 0)
            {
                // Primary screen so leave space for GPRCAD toolstrip menu and windows task bar
                screenAreaSizeX = rect.Width - 226;             // Allow 206 for GPRCAD toolstrip plus borders
                screenAreaSizeY = rect.Height - 250;            // Allow 200 for title bar and Windows Task bar
            }
            else
            {
                // Extended screen so use full screen size
                screenAreaSizeX = rect.Width - 20;
                screenAreaSizeY = rect.Height - 50;
            }

            // Calculate x and y scale of screen to site
            xScale = screenAreaSizeX / siteAreaSizeX;
            yScale = screenAreaSizeY / siteAreaSizeY;

            // Set scale for site to screen as smaller of the x or y scale above
            if (xScale > yScale)
            {
                scale = yScale;
                screenAreaSizeX = (Int32)(siteAreaSizeX * scale);
                if (screenAreaSizeX < 500)
                {
                    screenAreaSizeX = 500;
                }
            }
            else
            {
                scale = xScale;
                screenAreaSizeY = (Int32)(siteAreaSizeY * scale);
            }

            // Set window size
            this.Width = screenAreaSizeX + 18;                  // screen area size plus window border (2*9=18)
            this.Height = screenAreaSizeY + 45;                 // screen area height plus window border incl. menu strip

            // Re-adjust the site area to be scaled correctly
            siteAreaSizeX = glControl1.Width / scale;
            siteAreaSizeY = glControl1.Height / scale;

            if (siteAreaSizeX < siteAreaSizeY)
            {
                yScale = normalisedSize / siteAreaSizeY;
                viewWidth = (float)(siteAreaSizeX * yScale);
                viewDepth = (float)(siteAreaSizeZ * yScale * zScaleFactor);
                windowScale = dy / normalisedSize;
            }
            else
            {
                xScale = normalisedSize / siteAreaSizeX;
                viewHeight = (float)(siteAreaSizeY * xScale);
                viewDepth = (float)(siteAreaSizeZ * xScale * zScaleFactor);
                windowScale = dx / normalisedSize;
            }

            windowSize.Width = glControl1.Width;
            windowSize.Height = glControl1.Height;
            windowInitialised = true;

            SetModelView();

            this.Invalidate();
        }

        public Rectangle GetScreen()
        {
            return Screen.GetBounds(this);
        }

        private void SetModelView()
        {
            modelMatrix = Matrix4.CreateScale(zoom, zoom, zoom) * Matrix4.CreateRotationX(rotateAngleX) * Matrix4.CreateRotationY(rotateAngleY) * Matrix4.CreateRotationZ(rotateAngleZ) * Matrix4.CreateTranslation(moveX, moveY, 0f);
            projectionMatrix = Matrix4.CreateOrthographic(viewWidth, viewHeight, nearPlane, farPlane);
            lookat = Matrix4.LookAt(cameraPosition, cameraLookAt, cameraLookUp);
            ModelViewProjectionMatrix = lookat * modelMatrix * projectionMatrix;
        }

        #endregion Initialise

        #region Create Objects (lines and texture labels)

        private void CreateIndexedTexture()
        {
            Font txtFnt = new Font("Arial", 8);
            Color layerColor = Color.Cyan;
            SolidBrush txtBrsh = new SolidBrush(layerColor);
            Pen layerPen = new Pen(layerColor);
            Int32 textureWidth = 8, textureHeight = 12, numSubTextures = 10;

            string textLabel = "";

            // Create text Texture bitmap
            textureBitmap = new Bitmap(textureWidth, textureHeight * 10);

            using (Graphics gfx = Graphics.FromImage(textureBitmap))
            {
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                gfx.Clear(Color.Black);

                for (Int32 i = 0; i < 10; i++)
                {
                    textLabel = i.ToString();
                    gfx.DrawString(textLabel, txtFnt, txtBrsh, -1f, 0f + (textureHeight * i));
                }
            }

            textureBitmap.MakeTransparent(Color.Black);

            // Upload the Bitmap to OpenGL.

            // Generate texture id
            GL.GenTextures(1, out textTextureId);
            CheckForGL_Error("CreateIndexedTexture GenTextures textTextureId.");

            // Set Active Texture Unit
            GL.ActiveTexture(TextureUnit.Texture4);
            CheckForGL_Error("CreateIndexedTexture ActiveTexture set to Texture4.");

            // Bind to textureID
            GL.BindTexture(TextureTarget.Texture2DArray, textTextureId);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, textureWidth, textureHeight, numSubTextures);
            CheckForGL_Error("CreateIndexedTexture TexStorage3D");

            for (Int32 i = 0; i < 10; i++)
            {
                BitmapData bitmapData = textureBitmap.LockBits(new Rectangle(0, i * textureHeight, textureWidth, textureHeight), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, bitmapData.Width, bitmapData.Height, 1, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
                CheckForGL_Error("CreateIndexedTexture TexSubImage3D textureBitmap");

                textureBitmap.UnlockBits(bitmapData);

                //// Code to save image file
                //Bitmap tempBmp = textureBitmap.Clone(new Rectangle(0, i * textureHeight, textureWidth, textureHeight), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //try
                //{
                //    SaveImageFile(tempBmp, "IndexedTexture" + i.ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                //}
                //catch (System.Security.SecurityException)
                //{
                //    MessageBox.Show("File permissions do not allow you to create output file: IndexedTexture.jpg.");
                //}
                //// END Code to save image file
            }

            // Set Tex Parameters
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            CheckForGL_Error("CreateIndexedTexture TexParameter TextureMinFilter");

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            CheckForGL_Error("CreateIndexedTexture TexParameter TextureMagFilter");

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            CheckForGL_Error("CreateIndexedTexture TexParameter TextureWrapT");

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            CheckForGL_Error("CreateIndexedTexture TexParameter TextureWrapS");

            // UnBind textureID
            GL.BindTexture(TextureTarget.Texture2D, 0);
            CheckForGL_Error("CreateIndexedTexture TexSubImage3D textureBitmap");


            //// Code to save image file
            try
            {
                SaveImageFile(textureBitmap, "IndexedTexture.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (System.Security.SecurityException)
            {
                MessageBox.Show("File permissions do not allow you to create output file: IndexedTexture.jpg.");
            }
        }

        public void SaveImageFile(Bitmap imagebitmapImage, string filename, System.Drawing.Imaging.ImageFormat format)
        {
            FileStream streamOut;

            try
            {
                streamOut = new FileStream(filename, FileMode.Create);
            }
            catch (System.Security.SecurityException ex)
            {
                throw new System.Security.SecurityException(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }

            imagebitmapImage.Save(streamOut, format);

            streamOut.Close();
        }

        private void SetIndexedTextPosition(ref GPRscdGLTexture texture, decimal topLeftEasting, decimal topLeftNorthing, decimal bottomRightEasting, decimal bottomRightNorthing, decimal positionDepth)
        {
            vertex normalisedPosition;
            GLPositionText[] textPosition = new GLPositionText[4];

            normalisedPosition = normaliseVertex(new vertex(topLeftEasting, topLeftNorthing, (decimal)positionDepth));
            textPosition[0] = new GLPositionText((float)normalisedPosition.X, (float)normalisedPosition.Y, (float)0, 1, 0);
            texture.texturePosition[0] = textPosition[0];

            normalisedPosition = normaliseVertex(new vertex(bottomRightEasting, topLeftNorthing, (decimal)positionDepth));
            textPosition[1] = new GLPositionText((float)normalisedPosition.X, (float)normalisedPosition.Y, (float)0, 0, 0);
            texture.texturePosition[1] = textPosition[1];

            normalisedPosition = normaliseVertex(new vertex(bottomRightEasting, bottomRightNorthing, (decimal)positionDepth));
            textPosition[2] = new GLPositionText((float)normalisedPosition.X, (float)normalisedPosition.Y, (float)0, 0, 1);
            texture.texturePosition[2] = textPosition[2];

            normalisedPosition = normaliseVertex(new vertex(topLeftEasting, bottomRightNorthing, (decimal)positionDepth));
            textPosition[3] = new GLPositionText((float)normalisedPosition.X, (float)normalisedPosition.Y, (float)0, 1, 1);
            texture.texturePosition[3] = textPosition[3];
        }

        private vertex normaliseVertex(vertex a)
        {
            vertex b = new vertex();
            b.X = (a.X - (xMin + (decimal)halfdx)) / (decimal)(siteAreaSizeX / viewWidth);
            b.Y = (a.Y - (yMin + (decimal)halfdy)) / (decimal)(siteAreaSizeY / viewHeight);
            b.Z = (a.Z + (zMin + (decimal)halfdz)) / (decimal)(siteAreaSizeZ / viewDepth);
            return b;
        }

        #endregion Create Objects (lines and texture labels)

        #region Draw Objects

        private void ShowLineObjects()
        {
            // Clear previous verts amd indices
            lineVerts = null;
            lineIndices = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Int32 index = 0, lineLabel = 1;

            lineVertexCounter = 0;
            Int32 xSectionCount = lineObjects.Count();

            if (lineObjects.Count() > 0)
            {
                lineVerts = new GLPositionColored2[xSectionCount * 2];

                lineIndices = new UInt32[xSectionCount * 2];

                GLPositionColored2 vert;
                vertex normalisedPosition;

                foreach (LineObjects lineObject in lineObjects)
                {
                    vert = new GLPositionColored2();
                    normalisedPosition = normaliseVertex(lineObject.start);

                    vert.Position.X = (float)(normalisedPosition.X);
                    vert.Position.Y = (float)(normalisedPosition.Y);
                    vert.Position.Z = 0f;
                    vert.colour.X = 0;
                    vert.colour.Y = 1;
                    vert.colour.Z = 1;
                    lineVerts[lineVertexCounter] = vert;
                    lineIndices[index] = (UInt32)(lineVertexCounter);
                    index++;
                    lineVertexCounter++;

                    vert = new GLPositionColored2();
                    normalisedPosition = normaliseVertex(lineObject.end);

                    vert.Position.X = (float)(normalisedPosition.X);
                    vert.Position.Y = (float)(normalisedPosition.Y);
                    vert.Position.Z = 0f;
                    vert.colour.X = 0;
                    vert.colour.Y = 1;
                    vert.colour.Z = 1;
                    lineVerts[lineVertexCounter] = vert;
                    lineIndices[index] = (UInt32)(lineVertexCounter);
                    index++;
                    lineVertexCounter++;

                    // Draw line object reference numbers
                    DrawTextTexture(lineObject, lineLabel.ToString());

                    lineLabel++;
                }
            }
        }

        private void DrawTextTexture(LineObjects lineObject, string label)
        {
            decimal texturePosition = 0.0m;
            List<GPRscdGLTexture> lineTexture = null;
            Int32 textureIndex = 0;

            for (Int32 c = 0; c < label.Length; c++)
            {
                textureIndex = Convert.ToInt32(label.Substring(c, 1));

                GPRscdGLTexture texture = new GPRscdGLTexture();
                texture.text = textureIndex.ToString();
                texture.textureID = textTextureId;

                // Set the text position
                SetIndexedTextPosition(ref texture, lineObject.start.X + texturePosition, lineObject.start.Y, lineObject.start.X + 0.5m + texturePosition, lineObject.start.Y + 0.5m, 0m);

                lineTexture = (List<GPRscdGLTexture>)textTextureList[textureIndex];
                if (lineTexture == null)
                {
                    lineTexture = new List<GPRscdGLTexture>();
                }
                lineTexture.Add(texture);

                textTextureList[textureIndex] = (Object)lineTexture;

                texturePosition += 0.55m;
            }
        }

        #endregion Draw Objects

        #region Resize

        private void OnResize(object sender, EventArgs e)
        {
            pnlTimeslice.Width = this.Width - 4;
            pnlTimeslice.Height = this.Height - 30;
            OnGL_Resize(sender, e);
        }

        private void OnGL_Resize(object sender, EventArgs e)
        {
            if (windowInitialised)
            {
                // If the panel/glControl has a height of zero (minimized) then return without doing any calculations otherwise we get some NaN values and window won't recover.
                if (pnlTimeslice.Height == 0)
                {
                    return;
                }

                float newSiteAreaSizeX = glControl1.Width / scale;
                float newSiteAreaSizeY = glControl1.Height / scale;

                SetupViewport();

                if (newSiteAreaSizeX != siteAreaSizeX)
                {
                    yScale = viewHeight / newSiteAreaSizeY;
                    viewWidth = (float)(newSiteAreaSizeX * yScale);
                    viewDepth = (float)(siteAreaSizeZ * yScale * zScaleFactor);
                }

                if (newSiteAreaSizeY != siteAreaSizeY)
                {
                    xScale = viewWidth / newSiteAreaSizeX;
                    viewHeight = (float)(newSiteAreaSizeY * xScale);
                    viewDepth = (float)(siteAreaSizeZ * xScale * zScaleFactor);
                }

                // Re-adjust the site area to be scaled correctly
                siteAreaSizeX = newSiteAreaSizeX;
                siteAreaSizeY = newSiteAreaSizeY;

                windowSize.Width = glControl1.Width;
                windowSize.Height = glControl1.Height;

                SetupViewport();

                UpdateFrame();
                UpdateTextureFrame();

                SetModelView();
                glControl1.Invalidate();
            }
        }

        #endregion Resize

        #region Update Frame

        private void UpdateFrame()
        {
            // Draw xSection Lines
            if (lineIndices != null)
            {
                // Which buffer to draw
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOlines);
                CheckForGL_Error("UpdateFrame: BindBuffer ArrayBuffer VBOlines.");

                // Draw buffer
                GL.BufferData<GLPositionColored2>(BufferTarget.ArrayBuffer, (IntPtr)(lineVerts.Count() * GLPositionColored2.sizeInBytes), lineVerts.ToArray(), BufferUsageHint.StaticDraw);
                CheckForGL_Error("UpdateFrame: BufferData GLPositionColored2 ArrayBuffer lineVerts.");

                //Which buffer to draw
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, linesIBO);
                CheckForGL_Error("UpdateFrame: BindBuffer ElementArrayBuffer linesIBO.");

                //Draw buffer
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(lineIndices.Count() * sizeof(Int32)), lineIndices.ToArray(), BufferUsageHint.StaticDraw);
                CheckForGL_Error("UpdateFrame: BufferData ElementArrayBuffer lineIndices.");

                // Clear element array buffer binding
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                CheckForGL_Error("UpdateFrame: BindBuffer ElementBuffer 0.");

                // Clear array buffer binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                CheckForGL_Error("UpdateFrame: BindBuffer ArrayBuffer 0.");
            }

        }

        private void UpdateTextureFrame()
        {
            Int32 totalTextures = 0;
            List<GPRscdGLTexture> textTexture = null;

            for (Int32 t = 0; t < textTextureList.Length; t++)
            {
                textTexture = (List<GPRscdGLTexture>)textTextureList[t];
                if (textTexture != null)
                {
                    totalTextures += textTexture.Count();
                }
            }

            if (totalTextures > 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOtext);
                CheckForGL_Error("UpdateObjectTextFrame BindBuffer VBOobjectText");

                textPosition = new GLPositionText[totalTextures * 4];

                Int32 j = 0;

                // Loop through object texture list and copy texture positions to position array.
                for (Int32 t = 0; t < textTextureList.Length; t++)
                {
                    textTexture = (List<GPRscdGLTexture>)textTextureList[t];

                    if (textTexture != null)
                    {
                        for (Int32 i = 0; i < textTexture.Count(); i++)
                        {
                            textPosition[j] = textTexture[i].texturePosition[0];
                            textPosition[j + 1] = textTexture[i].texturePosition[1];
                            textPosition[j + 2] = textTexture[i].texturePosition[2];
                            textPosition[j + 3] = textTexture[i].texturePosition[3];

                            j += 4;
                        }
                    }
                }

                // Draw buffer
                GL.BufferData<GLPositionText>(BufferTarget.ArrayBuffer, (IntPtr)(textPosition.Length * GLPositionText.sizeInBytes), textPosition, BufferUsageHint.StaticDraw);

                // Clear array buffer binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        #endregion Update Frame

        #region Render Screen

        private void OnGL_Paint(object sender, PaintEventArgs e)
        {
            drawing = true;

            Render();

            drawing = false;
        }

        private void Render()
        {
            CheckForGL_Error("Render process: Existing error.");

            // Clear screen to background colour
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            CheckForGL_Error("Render process: Clear depth and color buffer.");

            //GL.EnableClientState(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            CheckForGL_Error("Render process: Enable Blend");

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            CheckForGL_Error("Render process: BlendFunc");

            #region Draw Objects

            {
                RenderLineObjects();

                // Textures

                RenderTextObjects();
            }

            #endregion Draw objects

            // Swap buffers
            glControl1.SwapBuffers();
        }

        private void RenderLineObjects()
        {
            ErrorCode err;
            #region Draw Lines

            // Draw lines
            if (lineIndices != null)
            {
                // Which shader program to use
                GL.UseProgram(shaderProgramID);
                CheckForGL_Error("Draw Lines UseProgram shaderProgramID");

                // Send the modelView matrix
                GL.UniformMatrix4(uniform_mview, false, ref ModelViewProjectionMatrix);
                CheckForGL_Error("Draw Lines UniformMatrix4 uniform_mview");

                try
                {
                    // Position buffer to draw
                    GL.BindBuffer(BufferTarget.ArrayBuffer, VBOlines);
                    CheckForGL_Error("Draw Lines BindBuffer VBOlines");

                    // Which attribute variables are to be used in the shader
                    GL.VertexAttribPointer(attribute_color, 3, VertexAttribPointerType.Float, false, GLPositionColored2.sizeInBytes, 0);
                    CheckForGL_Error("Draw Lines VertexAttribPointer attribute_color");

                    GL.VertexAttribPointer(attribute_position, 3, VertexAttribPointerType.Float, false, GLPositionColored2.sizeInBytes, 3 * sizeof(float));
                    CheckForGL_Error("Draw Lines VertexAttribPointer attribute_position");

                    GL.EnableVertexAttribArray(attribute_position);
                    CheckForGL_Error("Draw Lines EnableVertexAttribArray attribute_position");

                    GL.EnableVertexAttribArray(attribute_color);
                    CheckForGL_Error("Draw Lines EnableVertexAttribArray attribute_color");

                    //Index buffer to draw lines
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, linesIBO);
                    CheckForGL_Error("Draw Lines BindBuffer Element Array linesIBO");

                    GL.DrawElements(BeginMode.Lines, lineIndices.Count(), DrawElementsType.UnsignedInt, 0);
                    CheckForGL_Error("Draw Lines DrawElements lineIndices");

                    GL.DisableVertexAttribArray(attribute_position);
                    CheckForGL_Error("Draw Lines DisableVertexAttribArray attribute_position");

                    GL.DisableVertexAttribArray(attribute_color);
                    CheckForGL_Error("Draw Lines DisableVertexAttribArray attribute_color");

                    // Clear array buffer binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    CheckForGL_Error("Draw Lines BindBuffer clear array buffer");

                    // Clear element array buffer binding
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                    CheckForGL_Error("Draw Lines BindBuffer clear Element Array buffer");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error displaying Object lines.");
                }

                GL.UseProgram(0);
            }

            #endregion Draw Lines

        }

        private void RenderTextObjects()
        {
            ErrorCode err;
            List<GPRscdGLTexture> textTexture = null;

            #region Draw Line Numbers text

            for (Int32 t = 0; t < textTextureList.Length; t++)
            {
                textTexture = (List<GPRscdGLTexture>)textTextureList[t];

                if (textTexture != null)
                {
                    if (textTexture.Count() > 0)
                    {
                        GL.UseProgram(textShaderProgramID);
                        CheckForGL_Error("Draw Text UseProgram");

                        try
                        {
                            // Send the modelView matrix
                            GL.UniformMatrix4(uniform_modelview, false, ref ModelViewProjectionMatrix);
                            CheckForGL_Error("Draw XSection Texts UniformMatrix4");

                            GL.ActiveTexture(TextureUnit.Texture4);
                            CheckForGL_Error("Render Text ActiveTexture set to Texture4.");

                            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOtext);
                            CheckForGL_Error("BindBuffer VBOtext");

                            GL.VertexAttribPointer(attribute_texture_position, 3, VertexAttribPointerType.Float, false, GLPositionText.sizeInBytes, 0);
                            CheckForGL_Error("Render Text VertexAttribPointer attribute_texture_position");

                            GL.EnableVertexAttribArray(attribute_texture_position);
                            CheckForGL_Error("Render Text EnableVertexAttribArray attribute_texture_position");

                            GL.VertexAttribPointer(attribute_textture_coord, 2, VertexAttribPointerType.Float, false, GLPositionText.sizeInBytes, 3 * sizeof(float));
                            CheckForGL_Error("Render Text set VertexAttribPointer attribute_textture_coord");

                            GL.EnableVertexAttribArray(attribute_textture_coord);
                            CheckForGL_Error("Render Text EnableVertexAttribArray attribute_textture_coord");

                            Int32 textureCount = 0;

                            for (Int32 textureIndex = 0; textureIndex < textTextureList.Length; textureIndex++)
                            {
                                textTexture = (List<GPRscdGLTexture>)textTextureList[textureIndex];

                                if (textTexture != null)
                                {
                                    GL.BindTexture(TextureTarget.Texture2DArray, textTexture[0].textureID);
                                    CheckForGL_Error("Render Text BindTexture Texture2DArray textTexture.textureID");

                                    GL.Uniform1(uniform_texture_layer, textureIndex);
                                    CheckForGL_Error("Render Text Uniform1 set uniform_text_tex = " + textureIndex.ToString());

                                    GL.DrawArrays(PrimitiveType.Quads, textureCount, textTexture.Count() * 4);
                                    CheckForGL_Error("Render Text DrawArrays for texture textureBitmap layer " + textureIndex.ToString());

                                    textureCount += 4;
                                }
                            }

                            GL.BindTexture(TextureTarget.Texture2DArray, 0);
                            CheckForGL_Error("Render Text BindTexture unbind textTextureID");

                            GL.DisableVertexAttribArray(attribute_texture_position);
                            CheckForGL_Error("Render Text DisableVertexAttribArray attribute_texture_position");

                            GL.DisableVertexAttribArray(attribute_textture_coord);
                            CheckForGL_Error("Render Text DisableVertexAttribArray attribute_textture_coord");

                            // Clear array buffer bindings
                            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                            CheckForGL_Error("Render Text BindBuffer Clear ArrayBuffer");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error drawing text, msg: " + ex.Message + ".");
                        }

                        GL.UseProgram(0);
                    }
                }
            }

            #endregion Draw XSection Numbers text

        }

        #endregion Render Screen

        private void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                Render();
            }
        }

        private void CheckForGL_Error(string function)
        {
            ErrorCode err = GL.GetError();
            if (err != 0)
            {
                MessageBox.Show("A GL error was caught during the " + function + " function, error code: " + err.ToString() + ".");
                Console.WriteLine("A GL error was caught during the " + function + " function, error code: " + err.ToString() + ".");
            }
        }
    }

    #region Definitions

    /// <summary>
    /// Vertex struct holds position data for a 3D vertex
    /// </summary>
    [Serializable]
    public struct vertex
    {
        public decimal X;                                        // X co-ordinate
        public decimal Y;                                        // Y co-ordinate
        public decimal Z;                                        // Z co-ordinate

        public vertex(decimal x, decimal y, decimal z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static vertex operator +(vertex a, vertex b)
        {
            return new vertex(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static vertex operator -(vertex a, vertex b)
        {
            return new vertex(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static decimal operator ^(vertex p1, vertex p2)
        {
            return p1.X * p2.Y - p1.Y * p2.X;
        }

        public static bool operator <(vertex p1, vertex p2)
        {
            if (p1.Y == 0 && p1.X > 0) return true; //angle of p1 is 0, thus p2>p1
            if (p2.Y == 0 && p2.X > 0) return false; //angle of p2 is 0 , thus p1>p2
            if (p1.Y > 0 && p2.Y < 0) return true; //p1 is between 0 and 180, p2 between 180 and 360
            if (p1.Y < 0 && p2.Y > 0) return false;
            return (p1 ^ p2) > 0m; //return true if p1 is clockwise from p2
        }

        public static bool operator >(vertex p1, vertex p2)
        {
            // dummy operator
            return false;
        }

        public bool isEqual(vertex b)
        {
            if (X == b.X && Y == b.Y && Z == b.Z)
                return true;
            return false;
        }

        public bool notEqual(vertex b)
        {
            if (X != b.X || Y != b.Y || Z != b.Z)
                return true;
            return false;
        }

        public bool setMax()
        {
            X = decimal.MaxValue;
            Y = decimal.MaxValue;
            Z = decimal.MaxValue;

            return true;
        }

        public bool isMax()
        {
            if (X == decimal.MaxValue && Y == decimal.MaxValue && Z == decimal.MaxValue)
                return true;

            return false;
        }

        public bool isZero()
        {
            if (X == 0m && Y == 0m && Z == 0m)
                return true;

            return false;
        }

        public void setZero()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
    }

    public class GPRscdGLIndexedTextureBitmap
    {
        public Int32 textureID { get; set; }
        public Bitmap textureBitmap { get; set; }

        public GPRscdGLIndexedTextureBitmap()
        {
            textureID = 0;
        }
    }

    public class GPRscdGLTextureBitmap
    {
        public Int32 textureID { get; set; }
        public Bitmap textureBitmap { get; set; }
        public string tag { get; set; }
        public Int32 xSectionID { get; set; }
        public Int32 pipeSectionID { get; set; }
        public UInt32 targetID { get; set; }
        public string text { get; set; }

        public GPRscdGLTextureBitmap()
        {
            textureID = 0;
            tag = "";
            targetID = 0;
            text = "";
            xSectionID = pipeSectionID = -1;                    // Default is -1 for tagID, xSectionID and pipeSectionID
        }
    }

    public class GPRscdGLTexture
    {
        public Int32 textureID { get; set; }
        public string text { get; set; }
        public GLPositionText[] texturePosition;

        public GPRscdGLTexture()
        {
            texturePosition = new GLPositionText[4];
            textureID = 0;
            text = "";
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct GLPositionColored2
    {
        public Vector3 colour;
        public Vector3 Position;

        public static int sizeInBytes = 6 * sizeof(float);

        public GLPositionColored2(float posX, float posY, float posZ, float colR, float colG, float colB)
        {
            colour.X = colR;
            colour.Y = colG;
            colour.Z = colB;
            Position.X = posX;
            Position.Y = posY;
            Position.Z = posZ;
        }

        public GLPositionColored2(float posX, float posY, float posZ, Color col)
        {
            colour.X = col.R;
            colour.Y = col.G;
            colour.Z = col.B;
            Position.X = posX;
            Position.Y = posY;
            Position.Z = posZ;
        }

        public GLPositionColored2(vertex v)
        {
            colour.X = 0;
            colour.Y = 0;
            colour.Z = 0;
            Position.X = (float)v.X;
            Position.Y = (float)v.Y;
            Position.Z = (float)v.Z;
        }

        public GLPositionColored2(Vector3d v)
        {
            colour.X = 0;
            colour.Y = 0;
            colour.Z = 0;
            Position.X = (float)v.X;
            Position.Y = (float)v.Y;
            Position.Z = (float)v.Z;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct GLPositionText
    {
        public Vector3 position;
        public Vector2 texturePosition;

        public static int sizeInBytes = 5 * sizeof(float);

        public GLPositionText(float posX, float posY, float posZ, float texPosX, float texPosY)
        {
            position.X = posX;
            position.Y = posY;
            position.Z = posZ;
            texturePosition.X = texPosX;
            texturePosition.Y = texPosY;
        }
    }

    public class LineObjects
    {
        public vertex start { get; set; }
        public vertex end { get; set; }

        public LineObjects()
        { }

        public LineObjects(vertex s, vertex e)
        {
            start = s;
            end = e;
        }
    }

    #endregion Definitions

}
