import cs from 'classnames';
import React, { useState } from 'react';
import { useSnackbar } from 'notistack';
import { useSearchParams, useParams, useNavigate } from 'react-router-dom';
import { ArrowDropDownOutlined, Done, CloseOutlined } from '@material-ui/icons';
import {
  Grid,
  Button,
  useTheme,
  TextField,
  Typography,
  useMediaQuery,
  InputAdornment
} from '@material-ui/core';

import { useFormBuilder } from 'redux/hooks';
import useStyles from './styles';
import Dialog from './Dialog';

function FormSectionList() {
  const theme = useTheme();
  const classes = useStyles();
  const { formId } = useParams();
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const [showNewSectionField, setShowNewSectionField] = useState(false);
  const [newSectionName, setNewSectionName] = useState('');
  const [newSectionError, setNewSectionError] = useState('');
  const [deleteSectionId, setdeleteSectionId] = useState(null);
  const [isOpen, setIsOpen] = useState(false);
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [showSettings, setShowSettings] = useState(!isMobile);
  const questionGroup = searchParams.get('qg');
  const { questionsGroups, saveNewSectionData, deleteSectionData } = useFormBuilder();

  const questionsGroupsToShow = questionsGroups.filter(
    questionsGroup => questionsGroup.IsDeleted === false
  );

  const firstSectionId = questionsGroupsToShow.length ? questionsGroupsToShow[0].ID : -1;

  const toggleShowSettings = () => {
    if (isMobile) setShowSettings(!showSettings);
  };

  const handleDelete = async key => {
    const index = questionsGroups.findIndex(questionGroup => questionGroup.ID === key);

    const updatedQuestionGroups = [...questionsGroups];
    updatedQuestionGroups[index] = { ...questionsGroups[index], IsDeleted: true };

    await deleteSectionData({
      FormSettings: { FormId: formId },
      FormQuestionGroup: { ...questionsGroups[index], IsDeleted: true },
      FormQuestion: null,
      FormQuestionGroups: updatedQuestionGroups
    }).then(() => enqueueSnackbar('Deleted.', { variant: 'success' }));
  };

  const addNewSectionHandler = async () => {
    await saveNewSectionData({
      FormQuestionGroup: {
        FormId: formId,
        Name: newSectionName
      }
    });
  };

  const handleDeleteSection = async () => {
    await handleDelete(deleteSectionId);
    handleCloseDialog();

    if (questionGroup == deleteSectionId) {
      if (questionsGroupsToShow.length > 0) {
        setSearchParams(`qg=${firstSectionId}`);
      } else {
        navigate(`/Org/form/edit/${formId}`);
      }
    }
  };

  const handleCloseDialog = () => {
    setdeleteSectionId(null);
    setIsOpen(false);
  };

  if (isMobile && showSettings) {
    return (
      <Button
        variant="outlined"
        className={classes.formSettingsButton}
        onClick={toggleShowSettings}
        data-testid={'formSettingsButton'}
      >
        Form Settings
        <ArrowDropDownOutlined style={{ width: '20px', height: 'auto', marginLeft: '2px' }} />
      </Button>
    );
  }

  const cancelNewSectionHandler = () => {
    setShowNewSectionField(false);
    setNewSectionName('');
    setNewSectionError('');
  };

  const addNewSection = async () => {
    const isNameUnique = questionsGroupsToShow.every(({ Name: name }) => name !== newSectionName);

    if (newSectionName.length === 0) setNewSectionError('Section Name cannot be empty');
    else if (!isNameUnique) setNewSectionError('Section Name must be unique');
    else {
      setShowNewSectionField(false);
      setNewSectionError('');
      setNewSectionName('');
      await addNewSectionHandler();
    }
  };

  return (
    <Grid item lg={2} md={2} xs={12} sm={6} className={classes.sectionListWrapper}>
      <Typography
        variant="h6"
        className={classes.settingsLink}
        onClick={() => {
          navigate(`/Org/form/edit/${formId}`);
          toggleShowSettings();
        }}
      >
        Settings
      </Typography>
      <Typography
        variant="h6"
        className={classes.sectionsListHeading}
        onClick={() => {
          setSearchParams(`qg=${firstSectionId}`);
          toggleShowSettings();
        }}
        data-testid={'formSections'}
      >
        Sections
      </Typography>
      {questionsGroupsToShow.map(({ ID: key, QuestionGroupDataTypeId, Name }) => (
        <Grid item key={key} spacing={2} className={classes.displayFlex}>
          <Grid lg={11}>
            <Typography
              variant="body1"
              className={questionGroup == key ? classes.currentSection : classes.sectionName}
              onClick={() => {
                setSearchParams(`qg=${key}`);
                toggleShowSettings();
              }}
              data-testid={`formSection-${Name}`}
            >
              {Name}
            </Typography>
          </Grid>
          <Grid lg={1}>
            {QuestionGroupDataTypeId === 0 && (
              <CloseOutlined
                data-testid={`deleteFormSectionIcon-${Name}`}
                style={{ cursor: 'pointer' }}
                onClick={() => {
                  setIsOpen(true);
                  setdeleteSectionId(key);
                }}
              />
            )}
          </Grid>
        </Grid>
      ))}
      <Dialog
        open={isOpen}
        onClose={handleCloseDialog}
        title={'Delete Section'}
        text={'Do you want to delete this section? This action is irreversible.'}
        onSubmit={handleDeleteSection}
        dataTestId={'deleteSection'}
      />
      {showNewSectionField ? (
        <TextField
          className={cs(classes.formControlClass, classes.newSectionName)}
          data-testid={'newSectionNameField'}
          size="small"
          fullWidth
          key="newSectionField"
          placeholder="Section Name"
          value={newSectionName}
          onChange={event => setNewSectionName(event.target.value)}
          InputProps={{
            endAdornment: (
              <InputAdornment position="end">
                <Done
                  color="primary"
                  style={{ cursor: 'pointer' }}
                  onClick={addNewSection}
                  data-testid={'saveNewSectionNameFieldIcon'}
                />
                <CloseOutlined
                  data-testid={'closeNewSectionNameFieldIcon'}
                  color="error"
                  style={{ cursor: 'pointer' }}
                  onClick={cancelNewSectionHandler}
                />
              </InputAdornment>
            )
          }}
          helperText={newSectionError}
          error={newSectionError}
        />
      ) : (
        <Button
          color="primary"
          variant="outlined"
          style={{
            color: '#494949',
            fontSize: '14px',
            fontWeight: 'bold',
            border: '1px solid #E4E4E4'
          }}
          type="submit"
          disabled={false}
          onClick={async event => {
            event.preventDefault();
            setShowNewSectionField(true);
          }}
          data-testid={'addNewSectionButton'}
        >
          + Add New Section
        </Button>
      )}
    </Grid>
  );
}

export default FormSectionList;
