import { Box, Button, IconButton } from '@material-ui/core';
import React, { useState, useEffect, useRef } from 'react';
import { useReactMediaRecorder } from 'react-media-recorder';

import useStyles from './styles';
import Timer from 'components/Timer';
import audioWave from 'assets/audio.svg';
import playButton from 'assets/playButton.svg';
import pauseButton from 'assets/pauseGreen.svg';
import DescriptionModal from 'components/DescriptionModal';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import ErrorBand from 'components/common/ErrorBand';

const AudioQuestion = ({ question, formik }) => {
  const classes = useStyles();
  const audioRef = useRef();
  const { touched, values, errors, setFieldValue } = formik;
  const { ID, Text: title, IsRequired: isRequired, Description: description } = question;

  const [audioAnswer, setAudioAnswer] = useState('');
  const [isTimerRunning, setTimerRunning] = useState(false);
  const [isDescriptionModal, setDescriptionModal] = useState(false);
  const [descriptionText, setDescriptionText] = useState(description || 'description');
  const [isPlaying, setPlaying] = useState(false);
  const [audioUrl, setAudioUrl] = useState();
  const {
    startRecording,
    stopRecording,
    mediaBlobUrl,
    error: recorderError,
    status,
    clearBlobUrl
  } = useReactMediaRecorder({
    audio: {
      autoGainControl: true,
      echoCancellation: true
    },
    blobPropertyBag: { type: 'audio/wav' }
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (typeof audioAnswer === 'string') return;
    setFieldValue(ID, audioAnswer);
  }, [audioAnswer]);

  useEffect(() => {
    if (typeof values[ID] !== 'string') return;
    setAudioUrl(values[ID]);
    setAudioAnswer(values[ID]);
  }, [values[ID]]);

  const handleDiscard = () => {
    setAudioAnswer('');
    setDescriptionText('');
    clearBlobUrl();
    audioRef.current.src = '';
  };

  const startRecordingAudio = () => {
    startRecording();
  };

  const stopRecordingAudio = async () => {
    stopRecording();
    setTimerRunning(false);
    handleAnswer();
  };

  const playRecorderAudio = () => {
    setPlaying(true);
    audioRef.current.play();
  };

  const pauseRecorderAudio = () => {
    setPlaying(false);
    audioRef.current.pause();
  };

  const handleDescriptionModal = () => {
    setDescriptionModal(!isDescriptionModal);
  };

  const handleDescriptionSave = value => {
    setDescriptionText(value);
  };

  const handleStopPlaying = () => {
    setPlaying(false);
  };

  const handleAnswer = async () => {
    if (!isRequired && !audioAnswer) return true;
    setLoading(true);
    setLoading(false);
    return [];
  };

  const convertBlobToFile = async media => {
    let audioFile = await fetch(media)
      .then(r => r.blob())
      .then(
        blobFile =>
          new File([blobFile], `formQuestionAnswer.${ID}_audio.mp3`, { type: 'audio/mp3' })
      );
    setAudioAnswer(audioFile);
    setAudioUrl(media);
  };

  useEffect(() => {
    if (!recorderError && status === 'recording') {
      setTimerRunning(true);
    }
  }, [status, recorderError]);

  useEffect(() => {
    if (mediaBlobUrl) {
      convertBlobToFile(mediaBlobUrl);
      setDescriptionModal(true);
    }
  }, [mediaBlobUrl]);

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <Box className={classes.wrapper}>
        <Box className={classes.audioWrapper}>
          <img src={audioWave} className={classes.audioWave} alt="Audio Wave" />
          {audioAnswer ? (
            <>
              {isPlaying ? (
                <IconButton onClick={pauseRecorderAudio}>
                  <img src={pauseButton} alt="Pause Button" className={classes.pauseButton} />
                </IconButton>
              ) : (
                <IconButton onClick={playRecorderAudio}>
                  <img src={playButton} alt="Play Button" />
                </IconButton>
              )}
              <Button onClick={handleDiscard} className={classes.recordButton}>
                Discard & Record Again
              </Button>
              {descriptionText && (
                <div className={classes.descriptionWrapper}>
                  <div className={classes.descriptionText}>{descriptionText}</div>
                  <Button
                    onClick={handleDescriptionModal}
                    className={classes.editDescriptionButton}
                  >
                    Edit description
                  </Button>
                </div>
              )}
              {!descriptionText && (
                <Button onClick={handleDescriptionModal} className={classes.editDescriptionButton}>
                  Add description
                </Button>
              )}
            </>
          ) : (
            <>
              <Timer isRunning={isTimerRunning} />
              {isTimerRunning ? (
                <Button onClick={stopRecordingAudio} className={classes.recordButton}>
                  Stop
                </Button>
              ) : (
                <>
                  <Button onClick={startRecordingAudio} className={classes.recordButton}>
                    Record
                  </Button>
                </>
              )}
            </>
          )}
        </Box>
        <audio
          src={audioUrl}
          controls
          ref={audioRef}
          className={classes.audioInput}
          onEnded={handleStopPlaying}
        />
        <DescriptionModal
          isOpen={isDescriptionModal}
          handleClose={handleDescriptionModal}
          handleDescriptionSave={handleDescriptionSave}
          text={descriptionText}
          title={descriptionText ? 'Edit Description' : 'Add Description'}
        />
      </Box>
      {touched[ID] && errors[ID] && <ErrorBand error={errors[ID]} />}
    </FieldWrapper>
  );
};

export default AudioQuestion;
