import React, { useRef, useState, useEffect } from 'react';
import { Button, Typography, Box, FormHelperText } from '@material-ui/core';

import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import { isValidFile } from 'constants';
import useStyles from './styles';

const VALID_EXTENSIONS = [
  'png',
  'jpeg',
  'jpg',
  'pdf',
  'txt',
  'docx',
  'mp4',
  'mp3',
  'mpeg',
  'plain'
];

const FileUpload = ({ formik, question }) => {
  const classes = useStyles();
  const uploadInputRef = useRef();
  const [fileName, setFileName] = useState('');
  const [uploadedStatus, setUploadedStatus] = useState(null);
  const { touched, values, errors, setFieldValue } = formik;
  const { ID, Text: title, IsRequired: isRequired, QuestionDataTypeId } = question;

  const handleClick = () => {
    uploadInputRef.current.click();
  };

  const handleUploadChange = event => {
    const file = event.target.files[0];
    if (file) {
      const validStatus = isValidFile(file, VALID_EXTENSIONS);
      if (validStatus) {
        setUploadedStatus(validStatus);
      } else {
        setFieldValue(ID, file);
        setFileName(file.name);
        setUploadedStatus('SUCCESS');
      }
    }
  };

  const parseFileName = () => {
    if (typeof values[ID] === 'string')
      try {
        return values[ID].split('?')[0]
          .split('/')
          .at(-1);
      } catch {
        return '';
      }
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
      <Box className={classes.outerWrapper}>
        <div className={classes.photoWrapper}>
          <div>Supported file types are .pdf,.txt,.docx,.png,.jpg,.jpeg,.mp4,.mp3</div>
          <Box className={classes.uploadButtonWrapper}>
            <Button
              data-testid={`${title}-${QuestionDataTypeId}-button`}
              className={classes.uploadButton}
              onClick={handleClick}
            >
              {fileName ? 'Discard & Upload New File' : 'Upload File'}
            </Button>
            {fileName && (
              <Typography data-testid={`${title}-${QuestionDataTypeId}-filename`}>
                {fileName}
              </Typography>
            )}
          </Box>
          <input
            type="file"
            accept=".pdf,.txt,.docx,.png,.jpg,.jpeg,.mp4,.mp3"
            ref={uploadInputRef}
            className={classes.inputFile}
            onChange={handleUploadChange}
            data-testid={`${title}-${QuestionDataTypeId}-input`}
          />
        </div>
        {((touched[ID] && errors[ID]) || uploadedStatus) && (
          <Box className={classes.flexCenter}>
            <FormHelperText
              data-testid={`${title}-${QuestionDataTypeId}-error`}
              className={classes.maxWidth}
              error
            >
              {uploadedStatus && (uploadedStatus === 'SUCCESS' ? errors[ID] : uploadedStatus)}
            </FormHelperText>
          </Box>
        )}
      </Box>
    </FieldWrapper>
  );
};

export default FileUpload;
