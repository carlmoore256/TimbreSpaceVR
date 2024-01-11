public class SMABuffer {

    public float[] buffer;
    public int index;
    public int length;
    public float sum;
    // public float Average { get { return this.sum / this.length; } }

    public SMABuffer(int length) {
        this.length = length;
        this.buffer = new float[length];
        this.index = 0;
        this.sum = 0f;
    }

    public void Add(float value) {
        this.sum -= this.buffer[this.index];
        this.buffer[this.index] = value;
        this.sum += value;
        this.index = (this.index + 1) % this.length;
    }

    public float Average() {
        return this.sum / this.length;
    }

}