# import the opencv library
import cv2

# define a video capture object
cap = cv2.VideoCapture(1)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1920)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 1080)

while (True):
    # Capture the video frame by frame
    ret, frame = cap.read()
    # rotation de 90°
    frame90 = cv2.rotate(frame, cv2.ROTATE_90_COUNTERCLOCKWISE)
    cv2.imshow('frame90', frame90)

    # target
    ROI = frame90[900:1020, 0:1080]    # [y1:y2, x1:x2]
    cv2.imshow('ROI', ROI)

    # 'q' to quit
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()