import React, { useRef, useState, useEffect } from 'react';
import { IconButton, Button, Dialog, Typography, Box } from '@material-ui/core';
import cs from 'classnames';

import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import photoPlaceholder from 'assets/photoPlaceholder.svg';
import DescriptionModal from 'components/DescriptionModal';
import ErrorBand from 'components/common/ErrorBand';
import WebCamera from 'components/WebCamera';
import { validFileSize } from 'constants';
import capture from 'assets/capture.svg';
import useStyles from './styles';

const PhotoQuestion = ({ question, formik }) => {
  const classes = useStyles();
  const captureInputRef = useRef();
  const uploadInputRef = useRef();
  const { touched, values, errors, setFieldValue } = formik;

  const { ID, Text: title, IsRequired: isRequired, Description: description } = question;

  const [isDescriptionModal, setDescriptionModal] = useState(false);
  const [descriptionText, setDescriptionText] = useState(description);
  const [imageUrl, setImageUrl] = useState(null);
  const [uploadImageModal, setUploadImageModal] = useState(false);
  const [uploadedFile, setUploadedFile] = useState('');
  const [isCameraOpen, setCameraOpen] = useState(false);
  const [currentVideoStream, setCurrentStream] = useState([]);
  const [isImageLoading, setImageLoading] = useState(false);
  const [uploadingStatus, setUploadingStatus] = useState('');

  useEffect(() => {
    if (typeof values[ID]?.Text !== 'string') return;
    setImageUrl(values[ID]?.Text || values[ID]?.File);
    setDescriptionText(values[ID]?.Description);

    setCameraOpen(false);
  }, [values[ID]]);

  const handleDiscard = () => {
    setImageUrl('');
    setUploadedFile('');
    setDescriptionText('');
    setFieldValue(ID, '');
    setCurrentStream([]);
    uploadInputRef.current.value = '';
    captureInputRef.current.value = '';
  };

  const handleImageLoad = () => setImageLoading(false);

  const handleCaptureClick = () => {
    navigator.mediaDevices
      .getUserMedia({ video: true })
      .then(stream => {
        stream.getTracks().forEach(track => track.stop());
        setCurrentStream([]);
        setCameraOpen(true);
      })
      .catch(() => {
        captureInputRef.current.click();
      });
  };

  const handleDescriptionModal = () => {
    setDescriptionModal(!isDescriptionModal);
  };

  const handleDescriptionSave = value => {
    setDescriptionText(value);
    setFieldValue(ID, { ...values[ID], Description: value });
  };

  const handleImageUpload = () => {
    uploadInputRef.current.click();
  };

  useEffect(() => {
    if (typeof uploadedFile === 'string') return;
    setFieldValue(ID, { File: uploadedFile, Description: values[ID]?.Description });
    setUploadingStatus('SUCCESS');
  }, [uploadedFile]);

  function isValidUpload(file) {
    if (!validFileSize(file)) {
      setUploadImageModal(false);
      setUploadingStatus('FAILED');
      return false;
    }
    return true;
  }

  const handleUploadChange = event => {
    const file = event.target.files[0];
    if (!isValidUpload(file)) return;

    setUploadedFile(file);
    const reader = new FileReader();
    reader.onload = async upload => {
      const result = await upload.target.result;
      setImageUrl(result);
      setUploadImageModal(false);
      setDescriptionModal(true);
    };
    reader.readAsDataURL(file);
  };

  const handleUploadModal = () => {
    setCameraOpen(false);
    setUploadImageModal(!uploadImageModal);
  };

  const toggleCamera = () => setCameraOpen(!isCameraOpen);

  const handleWebCamUpload = async imageUrl => {
    setImageUrl(imageUrl);
    const imageFile = await fetch(imageUrl)
      .then(res => res.blob())
      .then(
        blobFile =>
          new File([blobFile], `formQuestionAnswer.${ID}_photo.jpg`, { type: 'image/png' })
      );

    if (!isValidUpload(imageFile)) return;

    setUploadedFile(imageFile);
    setDescriptionModal(true);
    setCameraOpen(false);
  };

  useEffect(() => {
    if (!isCameraOpen && currentVideoStream) {
      currentVideoStream.forEach(current => current.stop());
    }
  }, [isCameraOpen]);

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <Box className={classes.outerWrapper}>
        <Box className={classes.wrapper}>
          <div className={classes.photoWrapper}>
            <div className={cs(classes.photoContent, { [classes.webCamera]: isCameraOpen })}>
              {isCameraOpen && (
                <WebCamera
                  setCapturedData={handleWebCamUpload}
                  handleCameraOpen={toggleCamera}
                  setVideoStream={setCurrentStream}
                />
              )}
              {!isCameraOpen && imageUrl && (
                <img
                  src={imageUrl}
                  className={classes.uploadImage}
                  alt="Upload"
                  onLoad={handleImageLoad}
                />
              )}
              {!imageUrl && !isCameraOpen && !isImageLoading && (
                <img src={photoPlaceholder} className={classes.uploadImage} alt="Upload2" />
              )}
              {!isCameraOpen && !imageUrl && !isImageLoading && (
                <div className={classes.captureButton}>
                  <IconButton onClick={handleCaptureClick}>
                    <img src={capture} alt="Capture" />
                  </IconButton>
                </div>
              )}
            </div>
            {imageUrl && !isImageLoading && (
              <Button className={classes.uploadButton} onClick={handleDiscard}>
                Discard & Retake
              </Button>
            )}
            {descriptionText && imageUrl && !isImageLoading && (
              <div className={classes.descriptionWrapper}>
                <div className={classes.descriptionText}>{descriptionText}</div>
                <Button onClick={handleDescriptionModal} className={classes.editDescriptionButton}>
                  Edit Caption
                </Button>
              </div>
            )}
            {!descriptionText && imageUrl && !isImageLoading && (
              <Button onClick={handleDescriptionModal} className={classes.editDescriptionButton}>
                Add Caption
              </Button>
            )}
            {!descriptionText && !imageUrl && !isImageLoading && (
              <>
                <div className={classes.orText}>or</div>
                <Button className={classes.uploadButton} onClick={handleUploadModal}>
                  Upload Photo
                </Button>
              </>
            )}
          </div>
          <input
            type="file"
            accept="image/*"
            capture="user"
            className={classes.inputFile}
            ref={captureInputRef}
            onChange={handleUploadChange}
          />
          <input
            type="file"
            accept="image/*"
            ref={uploadInputRef}
            className={classes.inputFile}
            onChange={handleUploadChange}
          />
          <DescriptionModal
            isOpen={isDescriptionModal}
            handleClose={handleDescriptionModal}
            handleDescriptionSave={handleDescriptionSave}
            text={descriptionText}
            title={descriptionText ? 'Edit Caption' : 'Add Caption'}
          />
          <Dialog
            open={uploadImageModal}
            onClose={handleUploadModal}
            classes={{
              paper: classes.modal
            }}
          >
            <div className={classes.modalContent}>
              <Typography variant="subtitle1">Upload Photo</Typography>
              <Button
                className={classes.selectImageButton}
                color="primary"
                variant="contained"
                fullWidth
                onClick={handleImageUpload}
              >
                Select Photo
              </Button>
            </div>
          </Dialog>
        </Box>
        {touched[ID] && errors[ID] && <ErrorBand error={errors[ID]} />}
        {uploadingStatus === 'FAILED' && <ErrorBand error="File size cannot be greater than 5MB" />}
      </Box>
    </FieldWrapper>
  );
};

export default PhotoQuestion;
