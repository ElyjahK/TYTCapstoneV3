package AggregateStats

class SwitchVpdAggService {
    static transactional = true //We will be saving to the database. Better let it roll back changes in case it fails.
    List<SwitchVpdService> sw = [];

    public SwitchVpdAggService(String railroadName, String yardName) {
        for (int s = 0; s <= 100; s++) {
            sw[s] = new SwitchVpdService(railroadName, yardName, s);
        }
    }
    public void ProcessLine(int swIndex, String line, String latestDateStr, double timeOfLogMessage){
        sw[swIndex].ProcessMessage(line, latestDateStr,timeOfLogMessage);
    }
}
