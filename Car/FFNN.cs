//<SD>
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Car
{
    //позволяет ссылаться на численное значение
    [Serializable]
    public class Signal
    {
        public float Value;//значение
        public Neuron SourceNeuron;//дополнительная ссылка на нейрон источник сигнала (если он не равен null)
        public int signal_ind;//номер в наборе входных сигналов или в слое нейронов
        public int layer_ind;//номер слоя в сети. у слоя входных данных индекс = -1
    }

    //нейрон - обрабатывает входы и вычисляет выходное значение
    [Serializable]
    public class Neuron
    {
        //дендрит нейрона
        [Serializable]
        public class Dendrite
        {
            public Signal Signal;//ссылка на источник сигнала
            public float Weight;//весовой коэффициент
            public float dW;//для алгоритма обратоного распространения
            //расчитывает выход дендрита
            public float GetOutput()
            {
                return Signal.Value * Weight;
            }
        }

        public readonly Signal Output;//выход нейрона

        internal Dendrite[] Dendrites;
        internal Dendrite BiasDendrite;

        public Neuron()
        {
            Output = new Signal { SourceNeuron = this };
        }

        //ВЫЧИСЛЕНИЕ

        //нелинейная ф-ция активации
        static float f(float sum)
        {
            //return -1 + 2 / (1 + (float)Math.Exp(-sum));
            return 1 / (1 + (float)Math.Exp(-sum));
        }

        //рассчет выходного значения
        public float Calculate()
        {
            float sum = 0;
            for (int i = 0, c = Dendrites.Length; i < c; i++)
            {
                sum += Dendrites[i].GetOutput();
            }
            sum += BiasDendrite.GetOutput();
            var x = f(sum);
            dy_ds = df_ds_at_f(x);

            Output.Value = x;

            return x;
        }

        //ДЛЯ АЛГОРИТМА ОБУЧЕНИЯ ОБРАТНЫМ РАСПРОСТРАНЕНИЕМ

        internal float dE_ds;//производная суммарной квадратичной ошибки нейронной сети по взвешенной сумме этого нейрона
        internal float dy_ds;//производная выходного значения нейрона по взвешенной сумме его входов
        //производная ф-ции активации
        static float df_ds_at_f(float f0)
        {
            //return 0.5f * (1 - f0 * f0);
            return f0 * (1 - f0);
        }

        //СОЕДИНЕНИЕ С ИСТОЧНИКАМИ СИГНАЛОВ

        //пересоединяет нейрон с указанными источниками сигнала
        public void Reconnect(Signal[] signals_)
        {
            Dendrites = new Dendrite[signals_.Length];

            var weightMagnitude = 10f / (signals_.Length + 1);
            //соединяем с небольшими случайными весами
            for (int i = 0, c = signals_.Length; i < c; i++)
            {
                Dendrites[i] = new Dendrite() { Signal = signals_[i], Weight = GetRandomWeight(weightMagnitude) };
            }
            BiasDendrite = new Dendrite() { Signal = NNHelper.UnitSignal, Weight = GetRandomWeight(weightMagnitude) };
        }
        //для генерации случайных весовых коэффициентов заданной амплитуды
        float GetRandomWeight(float A)
        {
            return A * (float)(1 - 2 * NNHelper.Rnd.NextDouble());
        }
    }
    [Serializable]
    public class Layer
    {//слой нейронов - помогает вычислять несколько нейронов за раз

        public Neuron[] Neurons;
        //конструктор
        public Layer(int numNeurons_, int layer_ind)
        {
            this.layer_ind = layer_ind;

            Neurons = new Neuron[numNeurons_];
            Outputs = new float[Neurons.Length];
            for (int i = 0; i < numNeurons_; i++)
            {
                var n = Neurons[i] = new Neuron();
                n.Output.signal_ind = i;
                n.Output.layer_ind = layer_ind;
            }
        }

        public Layer() { }

        public int layer_ind;

        //последний рассчитанный выходной вектор
        public float[] Outputs;

        //вычисляет все нейроны
        public void Calculate()
        {
            int c = Neurons.Length;
            for (int i = 0; i < c; i++)
            {
                Outputs[i] = Neurons[i].Calculate();
            }
        }
    }

    //хранит связанные между собой слои нейронов
    [Serializable]
    public class FFNN
    {
        Signal[] InputSignals;
        public Layer[] Layers;

        public FFNN() { }

        //возвращает число входов в сети
        public int NumInputs { get { return InputSignals.Length; } }

        //ИНИЦИАЛИЗАЦИЯ

        //создает сеть с фиксированной архитектурой (числом входов и числами нейронов в слоях)
        private void Init(int num_inputs, int[] layers_lengths, bool auto_connect)
        {
            InputSignals = NNHelper.GetSignalArr(num_inputs);
            for (int i = 0; i < InputSignals.Length; i++)
            {
                InputSignals[i].signal_ind = i;
                InputSignals[i].layer_ind = -1;
            }

            Layers = new Layer[layers_lengths.Length];
            for (int i = 0; i < layers_lengths.Length; i++)
            {
                var l = new Layer(layers_lengths[i], i);
                Layers[i] = l;
            }
            if (auto_connect)
            {
                FullyConnectNeurons();
            }
        }
        //конструктор - инициализирует сеть по строке. например "4, 3, 1" создаст 
        //сеть с 4мя входами, 3мя нейронами в 1м слое и 1м во втором (выходном) слое
        public FFNN(string s, bool auto_connect)
        {
            int[] arr = NNHelper.ParseIntArray(s);
            int[] arr1 = new int[arr.Length - 1];
            Array.Copy(arr, 1, arr1, 0, arr1.Length);
            Init(arr[0], arr1, auto_connect);

            structure_string = s;
        }

        public string structure_string;

        //возвращает нужный сигнал из нужного слоя
        public Signal GetSignal(int layer_ind, int signal_ind)
        {
            if (layer_ind == -1) return InputSignals[signal_ind];
            return Layers[layer_ind].Neurons[signal_ind].Output;
        }

        //ВЫЧИСЛЕНИЕ

        //последний рассчитанный выходной вектор
        public float[] Outputs { get { return Layers[Layers.Length - 1].Outputs; } }

        //вычисляет все слои по порядку
        public float[] Calculate(float[] inputs)
        {
            NNHelper.InsertFloats(InputSignals, inputs);
            int c = Layers.Length;
            for (int i = 0; i < c; i++)
            {
                Layers[i].Calculate();
            }

            return Outputs;
        }

        public void Learn(float[] desiredOutputs, float[] outputsLearnSpeed, float eta, Dictionary<string, float> otherParams)
        {
            var errors = NNHelper.CalcErrors(Outputs, desiredOutputs);
            float miu = 0.3f;
            if (otherParams != null)
            {
                string key;
                key = "error_threshold"; if (otherParams.ContainsKey(key))
                {
                    var error_threshold = otherParams[key];
                    errors = NNHelper.ZeroErrors(errors, error_threshold);
                }
                key = "miu"; if (otherParams.ContainsKey(key)) miu = otherParams[key];
            }

            if(outputsLearnSpeed!=null)
            for (int i = 0; i < errors.Length; i++)
            {
                errors[i] *= outputsLearnSpeed[i];
            }

            BackPropagation(errors, eta, miu);
        }

        //СОЕДИНЕНИЕ НЕЙРОНОВ В СЛОЯХ

        //соединяет указанный нейрон с указанными сигналами предыдущего слоя или со всеми сигналами, если specific_target_neurons==null
        public void ConnectNeuron(int layer_ind, int neuron_ind, List<int> specific_input_signals)
        {
            var neuron = Layers[layer_ind].Neurons[neuron_ind];

            Signal[] signals_ = null, signals = null;

            if (layer_ind == 0) signals_ = InputSignals;
            else signals_ = Array.ConvertAll(Layers[layer_ind - 1].Neurons, n => n.Output);

            if (specific_input_signals == null)
            {
                //полное подсоединение
                signals = signals_;
            }
            else
            {
                int c = specific_input_signals.Count;
                signals = new Signal[c];
                for (int i = 0; i < c; i++)
                {
                    //частичное подсоединение
                    int k = specific_input_signals[i];
                    signals[i] = signals_[k];
                }
            }

            neuron.Reconnect(signals);
        }

        //соеднияет соседние указанный слой с предыдущим полностью
        public void FullyConnectLayer(int layer_ind)
        {
            int count;
            if (layer_ind == 0) count = InputSignals.Length;
            else count = Layers[layer_ind - 1].Neurons.Length;

            for (int j = 0, c1 = Layers[layer_ind].Neurons.Length; j < c1; j++)//по нейронам слоя
            {
                ConnectNeuron(layer_ind, j, null);
            }
        }

        //соеднияет соседние слои полными соединениями
        public void FullyConnectNeurons()
        {
            for (int i = 0, c = Layers.Length; i < c; i++)//по слоям
            {
                FullyConnectLayer(i);
            }
        }

        //АЛГОРИТМ ОБУЧЕНИЯ
        #region backprop
        // выполняет один шаг алгоритма обратного распространения
        public void BackPropagation(float[] Errors, float eta, float miu)
        {
            //ПОДГОТОВИТЕЛЬНЫЙ ЭТАП

            var L = Layers[Layers.Length - 1];
            //подготовка к изменению весов в первом слое...
            for (int j = 0, num = L.Neurons.Length; j < num; j++)
            {
                var N = L.Neurons[j];
                N.dE_ds = Errors[j] * N.dy_ds;
            }
            //...и в других слоях
            for (int i = Layers.Length - 1; i > 0; i--)//для каждого слоя справа налево (по направлению ко входам сети)
            {
                L = Layers[i];
                for (int j_right = 0, c = L.Neurons.Length; j_right < c; j_right++)//цикл по нейронам в слое правее
                {
                    var N_right = L.Neurons[j_right];
                    for (int j = 0, num = N_right.Dendrites.Length; j < num; j++)//цикл по нейронам в слое левее
                    {
                        var N_left = N_right.Dendrites[j].Signal.SourceNeuron;
                        //вычисление того как влияет изменение выхода нейрона в левом слое влияет на изменение общей ошибки сети (Delta=dE/dy)
                        N_left.dE_ds +=//влияние нейрона в левом слое равно
                         N_right.dE_ds * //сумме влияний нейронов в правом слое (которые ближе к выходам сети)
                         N_right.Dendrites[j].Weight;//с учетом силы соединения между левыми и правыми нейронами
                    }
                }
            }

            //ЭТАП ИЗМЕНЕНИЯ ВЕСОВ

            //меняем весовый коэффициенты в каждом слое
            for (int i = Layers.Length - 1; i >= 0; i--)// 1. для каждого слоя справа налево (по направлению ко входам сети)
            {
                L = Layers[i];
                for (int j = 0, num = L.Neurons.Length; j < num; j++)// 2. для каждого нейрона в слое
                {
                    var N = Layers[i].Neurons[j];

#warning magic numbers (ffnn backpropagation) wMAX
                    float wMAX = 1000f;
                    //                    float dwMAX = 10f, dwMIN = 0.001f;

                    var dend = N.BiasDendrite;//дендрит от сигнала смещения
                    float k_grad = -eta * (1 - miu);

                    for (int k = -1; ; )// 3. для каждого веса
                    {
                        var grad = N.dE_ds * dend.Signal.Value;//компонета вектора градиента для данного весового коэффициента

                        var dw = k_grad * grad + miu * dend.dW;
                        //#warning magic tricks
                        //                        dw = Helper.LimitAbs(dw, dwMAX);
                        //                        dw = Helper.LimitAbsLower(dw, dwMIN);

                        dend.Weight += dw;//меняем вес
                        //#warning magic tricks
                        //                        dend.Weight *= 0.99f;

                        dend.dW = dw;//запоминаем изменение веса                

                        //ограничиваем значения весов
                        if (dend.Weight > wMAX) dend.Weight = wMAX;
                        else if (dend.Weight < -wMAX) dend.Weight = -wMAX;

                        if (++k >= N.Dendrites.Length) break;
                        dend = N.Dendrites[k];//переход к следующему весовому коэффициенту
                    } //end 3.

                    if (BiasOff)
                        N.BiasDendrite.Weight = 0;

                    N.dE_ds = 0;//очистка переменной для следующей итерации алгоритма
                }//end 2.
            }//end 1.
        }

        public bool BiasOff = false;
        #endregion
    }

    //вспомогательные функции
    public static class NNHelper
    {
        //переводит сигналы в обычный массив чисел
        public static float[] ConvertSignals(Signal[] signals_)
        {
            var res = new float[signals_.Length];
            for (int i = 0, c = signals_.Length; i < c; i++) res[i] = signals_[i].Value;
            return res;
        }
        //обновляет массив сигналов значениями из массива чисел
        public static void InsertFloats(Signal[] signalsDst_, float[] floatsSrc_)
        {
            if (floatsSrc_.Length != signalsDst_.Length) throw new Exception("floats_src.Length!=signals_dst.Length");
            for (int i = 0, c = signalsDst_.Length; i < c; i++) signalsDst_[i].Value = floatsSrc_[i];
        }
        //возвращает массив пустых сигналов
        public static Signal[] GetSignalArr(int len_)
        {
            var res = new Signal[len_];
            for (int i = 0, c = res.Length; i < c; i++) res[i] = new Signal();
            return res;
        }
        public static readonly Random Rnd = new Random(); //генератор случайных чисел
        public static readonly Signal UnitSignal = new Signal() { Value = 1 };//сигнал со значением 1 (выступает в роли сигнала смещения для нейрона)
        //вычисляет рассогласования между реальным и желаемым вектором значений
        public static float[] CalcErrors(float[] current_, float[] desired_)
        {
            if (current_.Length != desired_.Length) throw new Exception("public static float[] NN.CalcErrors(float[] Current, float[] Desired): Current.Length!=Desired.Length");
            var res = new float[current_.Length];
            for (int i = 0; i < current_.Length; i++) res[i] = current_[i] - desired_[i];
            return res;
        }
        //зануляет маленькие ошибки, чтоб не переобучать сеть
        public static float[] ZeroErrors(float[] errors, float threshold)
        {
            var res = new float[errors.Length];
            for (int i = 0; i < errors.Length; i++) res[i] = (Math.Abs(errors[i]) < threshold) ? 0 : errors[i];
            return res;
        }
        //парсит строку вида "1, 4, 5"
        public static int[] ParseIntArray(string list)
        {
            string[] split = list.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return Array.ConvertAll(split, s => int.Parse(s));
        }
    }   

}
//</SD>