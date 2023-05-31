import React, { useEffect, useRef, useState } from 'react';
import { Button, IconButton, Dialog, Typography, Box } from '@material-ui/core';

import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import videoPlaceholder from 'assets/videoPlaceholder.svg';
import DescriptionModal from 'components/DescriptionModal';
import playButtonWhite from 'assets/playButtonWhite.svg';
import VideoRecorder from 'components/VideoRecorder';
import recordButton from 'assets/recordButton.svg';
import pauseButton from 'assets/pauseWhite.svg';
import useStyles from './styles';

const VideoQuestion = ({ question, formik }) => {
  const classes = useStyles();
  const playVideoRef = useRef();
  const inputVideoRef = useRef();
  const { setFieldValue } = formik;
  const { ID, Text: title, IsRequired: isRequired, Description: description } = question;

  const [loading, setLoading] = useState(false);
  const [isDescriptionModal, setDescriptionModal] = useState(false);
  const [uploadVideoModal, setUploadVideoModal] = useState(false);
  const [videoUrl, setVideoUrl] = useState('');
  const [uploadedVideo, setUploadedVideo] = useState();
  const [isVideoPlaying, setVideoPlaying] = useState(false);
  const [descriptionText, setDescriptionText] = useState(description);
  const [isCameraOpen, setCameraOpen] = useState(false);
  const [currentVideoStream, setCurrentStream] = useState([]);
  const [recordingError, setRecordingError] = useState('');
  const [isRecording, setRecording] = useState(false);
  const [isVideoLoading, setVideoLoading] = useState(false);
  const [isCapture, setIsCapture] = useState(false);

  const handleDescriptionModal = () => {
    setDescriptionModal(!isDescriptionModal);
  };

  useEffect(() => {
    setFieldValue(ID, uploadedVideo);
  }, [uploadedVideo]);

  const handleVideoLoading = () => setVideoLoading(!isVideoLoading);

  const handleDiscard = () => {
    setVideoUrl('');
    setDescriptionText('');
    setCurrentStream([]);
    setUploadedVideo('');
    setRecordingError(false);
    setRecording(false);
    inputVideoRef.current.value = '';
  };

  const handleRecording = () => {
    navigator.mediaDevices
      .getUserMedia({ video: true })
      .then(stream => {
        stream.getTracks().forEach(track => track.stop());
        setCurrentStream([]);
        setCameraOpen(true);
      })
      .catch(() => {
        inputVideoRef.current.click();
      });
  };

  const handleDescriptionSave = value => {
    setDescriptionText(value);
  };

  const handlePlayRecording = () => {
    setVideoPlaying(true);
    playVideoRef.current.play();
  };

  const handleVideoModal = () => {
    if (isRecording) {
      setRecordingError('Video Recording is in progress');
      return;
    }
    setCameraOpen(false);
    setUploadVideoModal(!uploadVideoModal);
  };

  const handleVideo = flag => () => {
    if (flag) return handleRecording();
    setIsCapture(flag);
    setTimeout(() => {
      return inputVideoRef.current.click();
    }, 200);
  };

  const handleVideoChange = event => {
    const file = event.target.files[0];
    setUploadedVideo(file);
    const reader = new FileReader();
    reader.onload = async upload => {
      const result = await upload.target.result;
      setVideoUrl(result);
      setUploadVideoModal(false);
      setDescriptionModal(true);
    };
    reader.readAsDataURL(file);
  };

  const handleAnswer = async () => {
    if (!isRequired && !uploadedVideo) return true;
    setLoading(true);
    setLoading(false);
    return [];
  };

  const handleVideoPlay = () => {
    setVideoPlaying(!isVideoPlaying);
  };

  const handleVideoPause = () => {
    playVideoRef.current.pause();
    setVideoPlaying(false);
  };

  const handleWebCamUpload = async videoUrl => {
    let videoFile = await fetch(videoUrl)
      .then(r => r.blob())
      .then(
        blobFile =>
          new File([blobFile], `formQuestionAnswer_${ID}_video.mp4`, { type: 'video/mp4' })
      );
    setUploadedVideo(videoFile);
    setVideoUrl(videoUrl);
    setCameraOpen(false);
    setDescriptionModal(true);
    setRecording(false);
  };

  const handleStoreChange = async (answer, description) => {
    setDescriptionText(description);
    setUploadedVideo(answer);
    if (typeof answer === 'string') {
      setVideoUrl(answer);
    } else {
      const fileReader = new FileReader();
      fileReader.onload = async upload => {
        const result = await upload.target.result;
        setVideoUrl(result);
      };
      fileReader.readAsDataURL(answer);
    }
  };

  //   useEffect(() => {
  //     setDescriptionText('');
  //     setUploadedVideo('');
  //     const availableAnswer = surveyAnswers?.find(answer => answer.uid === uid);
  //     if (availableAnswer?.answer) {
  //       const { answer, description } = availableAnswer;
  //       handleStoreChange(answer, description);
  //     }
  //     // // eslint-disable-next-line react-hooks/exhaustive-deps
  //   }, [uid]);

  useEffect(() => {
    if (!isCameraOpen && currentVideoStream) {
      currentVideoStream.forEach(current => current.stop());
    }
    // // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isCameraOpen]);

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <Box style={{ display: 'flex' }}>
        <Box className={classes.videoWrapper}>
          <div className={classes.imgWrapper}>
            {videoUrl && !isCameraOpen && (
              <video
                src={videoUrl}
                style={{ width: 720 }}
                className={classes.videoPlayer}
                ref={playVideoRef}
                onEnded={handleVideoPlay}
                onLoadStart={handleVideoLoading}
                onLoadedData={handleVideoLoading}
                onClick={handleVideoPause}
              />
            )}
            {!videoUrl && isCameraOpen && (
              <VideoRecorder
                setCapturedData={handleWebCamUpload}
                isVideo={true}
                setVideoStream={setCurrentStream}
                setRecording={setRecording}
                recording={isRecording}
              />
            )}
            {!videoUrl && !isCameraOpen && (
              <img src={videoPlaceholder} className={classes.videoPlayer} alt="Video Placeholder" />
            )}
            {videoUrl && isVideoPlaying && !isVideoLoading && (
              <div className={classes.iconWrapper}>
                <IconButton onClick={handleVideoPause} className={classes.pauseButton}>
                  <img src={pauseButton} alt="Pause Button" />
                </IconButton>
              </div>
            )}
            {videoUrl && !isVideoPlaying && !isVideoLoading && (
              <div className={classes.playButtonWrapper}>
                <IconButton className={classes.playButton} onClick={handlePlayRecording}>
                  <img src={playButtonWhite} alt="Play Button" />
                </IconButton>
              </div>
            )}
            {!videoUrl && !isCameraOpen && !isVideoLoading && (
              <IconButton className={classes.recordButton} onClick={handleVideo(true)}>
                <img src={recordButton} alt="Record Button" />
              </IconButton>
            )}
          </div>
          {videoUrl && !isVideoLoading && (
            <Button className={classes.uploadButton} onClick={handleDiscard}>
              Discard & Record Again
            </Button>
          )}
          {descriptionText && (
            <div className={classes.descriptionWrapper}>
              <div className={classes.descriptionText}>{descriptionText}</div>
              <Button onClick={handleDescriptionModal} className={classes.editDescriptionButton}>
                Edit description
              </Button>
            </div>
          )}
          {!descriptionText && videoUrl && !isVideoLoading && (
            <Button onClick={handleDescriptionModal} className={classes.editDescriptionButton}>
              Add description
            </Button>
          )}
          {!descriptionText && !videoUrl && (
            <>
              <div className={classes.orText}>OR</div>
              <Button className={classes.uploadButton} onClick={handleVideoModal}>
                Upload Video
              </Button>
            </>
          )}
        </Box>
        <DescriptionModal
          isOpen={isDescriptionModal}
          handleClose={handleDescriptionModal}
          handleDescriptionSave={handleDescriptionSave}
          text={descriptionText}
          title={descriptionText ? 'Edit Description' : 'Add Description'}
        />
        <Dialog
          open={uploadVideoModal}
          onClose={handleVideoModal}
          classes={{
            paper: classes.modal
          }}
        >
          <div className={classes.modalContent}>
            <Typography variant="subtitle1">Upload Video</Typography>
            <Button
              className={classes.selectVideoButton}
              color="primary"
              variant="contained"
              fullWidth
              onClick={handleVideo(false)}
            >
              Select Video
            </Button>
          </div>
        </Dialog>
        <input
          type="file"
          accept="video/*"
          capture={isCapture}
          className={classes.videoInput}
          ref={inputVideoRef}
          onChange={handleVideoChange}
        />
      </Box>
    </FieldWrapper>
  );
};

export default VideoQuestion;
