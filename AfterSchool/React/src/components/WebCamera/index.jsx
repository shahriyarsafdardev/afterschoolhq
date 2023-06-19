import Webcam from 'react-webcam';
import React, { useEffect, useRef, useState } from 'react';
import { Button, useTheme, useMediaQuery } from '@material-ui/core';

import useStyles from './styles';

const WebCamera = ({
  setCapturedData,
  handleCameraOpen = () => {},
  setVideoStream = () => {},
  setCrop = () => {}
}) => {
  const webCamRef = useRef();
  const classes = useStyles();
  const theme = useTheme();
  const isTablet = useMediaQuery(theme.breakpoints.down('sm'));
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));
  const [videoConstraints, setVideoConstraints] = useState({});
  const [currentVideoStream, setCurrentStream] = useState([]);

  const getWidth = () => {
    if (isDesktop) return 675;
    if (isTablet) return 480;
  };

  const getHeight = () => {
    if (isDesktop) return 432;
    if (isTablet) return 480;
  };

  const handleCaptureClick = () => {
    const capture = webCamRef.current.getScreenshot();
    setCapturedData(capture);
    handleCameraOpen();
    setCrop(true);
    currentVideoStream.forEach(current => current.stop());
  };

  const handleUserMedia = stream => {
    const updatedStream = [...currentVideoStream];
    updatedStream.push(stream.getTracks()[0]);
    setCurrentStream(updatedStream);
    setVideoStream(updatedStream);
  };

  useEffect(() => {
    setVideoConstraints({
      width: getWidth(),
      height: getHeight(),
      facingMode: 'user'
    });
    // // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isDesktop, isTablet]);

  return (
    <div className={classes.webCamWrapper}>
      <Webcam
        audioConstraints={{
          autoGainControl: true,
          echoCancellation: true,
          noiseSuppression: true
        }}
        ref={webCamRef}
        screenshotFormat="image/jpeg"
        width={getWidth()}
        height={'auto'}
        videoConstraints={videoConstraints}
        onUserMedia={handleUserMedia}
        screenshotQuality={1}
      />
      <Button
        onClick={handleCaptureClick}
        variant="contained"
        className={classes.takePhoto}
        color="primary"
      >
        Capture
      </Button>
    </div>
  );
};

export default WebCamera;
