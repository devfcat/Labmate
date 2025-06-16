import serial
import json
import time
from datetime import datetime
import os

class DataCollector:
    def __init__(self, port='COM3', baud_rate=115200):
        self.serial = serial.Serial(port, baud_rate)
        self.experiment_name = datetime.now().strftime("%Y%m%d_%H%M%S")
        self.data_dir = "experiment_data"
        
        # 데이터 디렉토리 생성
        if not os.path.exists(self.data_dir):
            os.makedirs(self.data_dir)
            
        self.data_file = os.path.join(self.data_dir, f"{self.experiment_name}.json")
        self.data = []
        
    def start_collection(self):
        print(f"실험 시작: {self.experiment_name}")
        print("데이터 수집 중... (Ctrl+C로 종료)")
        
        try:
            while True:
                # MEASURE 명령 전송
                self.serial.write(b"MEASURE\n")
                
                # 데이터 수신
                line = self.serial.readline().decode().strip()
                
                if line.startswith("{"):
                    try:
                        data_point = json.loads(line)
                        data_point['timestamp'] = datetime.now().isoformat()
                        self.data.append(data_point)
                        
                        # 실시간으로 데이터 출력
                        print(f"전압: {data_point['voltage']:.3f}V, "
                              f"전류: {data_point['current']:.3f}mA, "
                              f"저항: {data_point['resistance']:.2f}Ω, "
                              f"오차: {data_point['error']:.2f}%")
                        
                        # 데이터 저장
                        self.save_data()
                        
                    except json.JSONDecodeError:
                        print("JSON 파싱 오류")
                
                time.sleep(1)  # 1초 대기
                
        except KeyboardInterrupt:
            print("\n데이터 수집 종료")
            self.serial.close()
            
    def save_data(self):
        with open(self.data_file, 'w', encoding='utf-8') as f:
            json.dump(self.data, f, ensure_ascii=False, indent=2)

if __name__ == "__main__":
    collector = DataCollector()
    collector.start_collection() 