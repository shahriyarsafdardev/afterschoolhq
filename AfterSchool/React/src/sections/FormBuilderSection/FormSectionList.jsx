import cs from 'classnames';
import React, { useState, useRef } from 'react';
import { useSnackbar } from 'notistack';
import { useSearchParams, useParams, useNavigate } from 'react-router-dom';
import { ArrowDropDownOutlined, Done, CloseOutlined } from '@material-ui/icons';
import {
  Box,
  Grid,
  Button,
  useTheme,
  TextField,
  Typography,
  useMediaQuery,
  InputAdornment,
  Popover
} from '@material-ui/core';

import { useUnsavedChanges } from './UnsavedChangesProvider';
import { useFormBuilder } from 'redux/hooks';
import useStyles from './styles';
import Dialog from './Dialog';

function FormSectionList() {
  const theme = useTheme();
  const classes = useStyles();
  const ref = useRef(null);
  const { formId } = useParams();
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();

  const [open, setOpen] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const [isUnsaved, setIsUnsaved] = useState(false);
  const [newSectionName, setNewSectionName] = useState('');
  const [newSearchParams, setNewSearchParams] = useState('');
  const [newSectionError, setNewSectionError] = useState('');
  const [deleteSectionId, setdeleteSectionId] = useState(null);
  const [showNewSectionField, setShowNewSectionField] = useState(false);

  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [showSettings, setShowSettings] = useState(!isMobile);
  const questionGroup = searchParams.get('qg');
  const { unsavedChanges } = useUnsavedChanges();
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

  const SectionList = () => (
    <Grid item lg={2} md={2} xs={12} sm={12} className={classes.sectionListWrapper}>
      {!isMobile && (
        <>
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
        </>
      )}
      {questionsGroupsToShow.map(({ ID: key, QuestionGroupDataTypeId, Name }) => (
        <Grid
          item
          lg={12}
          md={12}
          xs={12}
          sm={12}
          key={key}
          spacing={2}
          className={classes.displayFlex}
        >
          <Grid lg={11} md={11} xs={11} sm={11}>
            <Typography
              variant="body1"
              className={questionGroup == key ? classes.currentSection : classes.sectionName}
              onClick={() => {
                if (unsavedChanges) {
                  setNewSearchParams(`qg=${key}`);
                  setIsUnsaved(true);
                } else {
                  setSearchParams(`qg=${key}`);
                  toggleShowSettings();
                }
              }}
              data-testid={`formSection-${Name}`}
            >
              {Name}
            </Typography>
          </Grid>
          <Grid lg={1} md={1} xs={1} sm={1}>
            {QuestionGroupDataTypeId === 0 && (
              <CloseOutlined
                data-testid={`deleteFormSectionIcon-${Name}`}
                style={{ cursor: 'pointer', color: '#B2B2B4' }}
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
      <Dialog
        open={isUnsaved}
        onClose={() => {
          setNewSearchParams('');
          setIsUnsaved(false);
        }}
        title={'Unsaved Changes'}
        cancelButtonText="Stay on Page"
        confirmationButtonText="Leave This Page"
        text={
          'If you leave this page, any unsaved changes will be lost. Please close this window and click “Save” to keep progress.'
        }
        onSubmit={() => {
          setSearchParams(newSearchParams);
          setIsUnsaved(false);
        }}
        dataTestId={`deleteQuestion`}
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
            marginTop: '25px',
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

  if (isMobile) {
    return (
      <Box>
        <Typography className={classes.link} onClick={() => navigate(`/Org/form/edit/${formId}`)}>
          Return to Edit Settings
        </Typography>
        <Button
          variant="outlined"
          className={classes.formSettingsButton}
          onClick={() => setOpen(true)}
          data-testid={'formSettingsButton'}
          ref={ref}
        >
          Form Settings
          <ArrowDropDownOutlined style={{ width: '20px', height: 'auto', marginLeft: '2px' }} />
        </Button>
        <Popover
          anchorEl={ref.current}
          open={open}
          onClose={() => setOpen(false)}
          anchorOrigin={{
            vertical: 'bottom',
            horizontal: 'left'
          }}
          transformOrigin={{
            vertical: 'top',
            horizontal: 'left'
          }}
        >
          <SectionList />
        </Popover>
      </Box>
    );
  }

  return <SectionList />;
}

export default FormSectionList;
