import React, { useRef, useState, useEffect } from 'react';
import { Button, Typography, Box, FormHelperText } from '@material-ui/core';

import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import useStyles from './styles';

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB in bytes

const FileUpload = ({ formik, question: { ID, Text: title, IsRequired: isRequired } }) => {
  const classes = useStyles();
  const uploadInputRef = useRef();
  const [fileName, setFileName] = useState('');
  const [uploadedStatus, setUploadedStatus] = useState(null);
  const { touched, values, errors, setFieldValue } = formik;

  const handleClick = () => {
    uploadInputRef.current.click();
  };

  const handleUploadChange = event => {
    const file = event.target.files[0];
    if (file && file.size <= MAX_FILE_SIZE) {
      setFieldValue(ID, file);
      setFileName(file.name);
      setUploadedStatus('SUCCESS');
    } else {
      setUploadedStatus('FAILED');
    }
  };

  const parseFileName = () => {
    if (typeof values[ID] === 'string') return values[ID].split('/').at(-1);
  };
  const getFileName = () => values[ID]?.name || parseFileName() || '';

  useEffect(() => {
    setFileName(getFileName());
  }, []);

  useEffect(() => {
    setFileName(getFileName());
  }, [values[ID]]);

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <div className={classes.photoWrapper}>
        <div>Supported file types are .pdf,.txt,.docx,.png,.jpg,.jpeg,.mp4,.mp3</div>
        <Box className={classes.uploadButtonWrapper}>
          <Button className={classes.uploadButton} onClick={handleClick}>
            {fileName ? 'Discard & Upload New File' : 'Upload File'}
          </Button>
          {fileName && <Typography>{fileName}</Typography>}
        </Box>
        <input
          type="file"
          accept=".pdf,.txt,.docx,.png,.jpg,.jpeg,.mp4,.mp3"
          ref={uploadInputRef}
          className={classes.inputFile}
          onChange={handleUploadChange}
        />
      </div>
      {((touched[ID] && errors[ID]) || uploadedStatus === 'FAILED') && (
        <Box className={classes.flexCenter}>
          <FormHelperText className={classes.maxWidth} error>
            {uploadedStatus === 'FAILED' ? 'File size cannot be greater than 5MB' : errors[ID]}
          </FormHelperText>
        </Box>
      )}
    </FieldWrapper>
  );
};

export default FileUpload;
