import React, { useEffect, useRef, useMemo, useState } from 'react';
import { Button } from '@material-ui/core';
import { useTheme, useMediaQuery } from '@material-ui/core';

import useStyles from './styles';

const getDeviceType = () => {
  const ua = navigator.userAgent;
  if (/(tablet|ipad|playbook|silk)|(android(?!.*mobi))/i.test(ua)) {
    return 'tablet';
  }

  return 'desktop';
};

const VideoRecorder = ({
  setCapturedData,
  handleCameraOpen = () => {},
  setCrop = () => {},
  setRecording = () => {},
  isVideo = false
}) => {
  const webCamRef = useRef();
  const classes = useStyles();
  const theme = useTheme();
  const isTablet = useMediaQuery(theme.breakpoints.down('sm'));
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));
  const [currentVideoStream, setCurrentStream] = useState([]);
  const [capturing, setCapturing] = useState(false);

  const [source, setSource] = useState(null);
  const [mediaRecorder, setMediaRecorder] = useState(null);
  const chunks = useRef([]);
  const videoElement = useRef();

  const device = useMemo(getDeviceType, []);
  const getWidth = () => {
    if (isDesktop) return 675;
    if (isTablet) return 480;
  };

  const handleCaptureClick = () => {
    const capture = webCamRef.current.getScreenshot();
    setCapturedData(capture);
    handleCameraOpen();
    setCrop(true);
    currentVideoStream.forEach(current => current.stop());
  };

  useEffect(() => {
    navigator.mediaDevices
      .getUserMedia({
        video: {
          width: {
            min: 510,
            max: 1920
          },
          height: 'auto'
        },
        audio: {
          echoCancellation: true
        }
      })
      .then(setSource)
      .catch(e => console.log(`error`, e));
    //// eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (source) {
      videoElement.current.srcObject = source;
      const mr = new MediaRecorder(source, {
        mimeType: 'video/webm'
      });
      mr.onstart = () => {
        chunks.current = [];
        setRecording(true);
        setCapturing(true);
      };
      mr.ondataavailable = e => chunks.current.push(e.data);
      mr.onstop = () => {
        const blob = new Blob(chunks.current, { type: 'video/webm' });
        const url = URL.createObjectURL(blob);
        source.getTracks().forEach(current => current.stop());
        setCapturedData(url);
        setRecording(false);
        setCapturing(false);
      };
      setMediaRecorder(mr);
    } else {
      videoElement.current.srcObject = null;
      setMediaRecorder(null);
      setRecording(false);
    }
    //// eslint-disable-next-line react-hooks/exhaustive-deps
  }, [videoElement, source]);

  return (
    <div className={classes.webCamWrapper}>
      {device === 'desktop' && (
        <video width={getWidth()} height={'auto'} autoPlay ref={videoElement} />
      )}

      {isVideo ? (
        capturing ? (
          <Button
            onClick={() => mediaRecorder.stop()}
            variant="contained"
            className={classes.takePhoto}
            color="primary"
          >
            Stop Recording
          </Button>
        ) : (
          <Button
            onClick={() => mediaRecorder.start()}
            variant="contained"
            className={classes.takePhoto}
            color="primary"
          >
            Start Recording
          </Button>
        )
      ) : (
        <Button
          onClick={handleCaptureClick}
          variant="contained"
          className={classes.takePhoto}
          color="primary"
        >
          Capture
        </Button>
      )}
    </div>
  );
};

export default VideoRecorder;
