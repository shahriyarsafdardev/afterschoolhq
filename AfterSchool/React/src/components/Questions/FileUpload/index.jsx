import React, { useRef, useState, useEffect } from 'react';
import { Button, Typography, Box } from '@material-ui/core';

import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import useStyles from './styles';

const FileUpload = ({ formik, question: { ID, Text: title, IsRequired: isRequired } }) => {
  const classes = useStyles();
  const uploadInputRef = useRef();
  const [fileName, setFileName] = useState('');
  const [uploadedFile, setUploadedFile] = useState('');
  const { touched, values, errors, setFieldValue } = formik;

  const handleClick = () => {
    setFileName('');
    setUploadedFile('');
    uploadInputRef.current.value = '';
    uploadInputRef.current.click();
  };

  const handleUploadChange = event => {
    const file = event.target.files[0];
    setUploadedFile(file);
    setFileName(file.name);
  };

  useEffect(() => {
    setUploadedFile('');
    setFileName('');
  }, []);

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <div className={classes.photoWrapper}>
        <div>Supported file types are .pdf,.txt,.docx,.png,.jpg,.jpeg,.mp4,.mp3</div>
        <Box className={classes.uploadButtonWrapper}>
          <Button className={classes.uploadButton} onClick={handleClick}>
            {uploadedFile ? 'Discard & Upload New File' : 'Upload File'}
          </Button>
          {fileName && <Typography>{fileName}</Typography>}
        </Box>
        <input
          type="file"
          accept="*/*"
          ref={uploadInputRef}
          className={classes.inputFile}
          onChange={handleUploadChange}
        />
      </div>
      {touched[ID] && errors[ID] && (
        <Box className={classes.flexCenter}>
          <FormHelperText className={classes.maxWidth} error>
            {errors[ID]}
          </FormHelperText>
        </Box>
      )}
    </FieldWrapper>
  );
};

export default FileUpload;
